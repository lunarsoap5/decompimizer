import argparse
import logging
import os
import shutil
import sys
from io import BytesIO
from pathlib import Path

"""
Script to rebuild a decompiled Twilight Princess ISO by replacing REL files and the main DOL
with decompiled versions from the build directory.
"""

sys.path.insert(0, os.path.join(os.path.dirname(__file__), "gclib"))

from gclib.gcm import GCM
from gclib.rarc import RARC
from gclib.yaz0_yay0 import Yaz0

logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')

# Constants for file paths
RELS_ARC_PATH = "files/RELS.arc"
MAIN_DOL_PATH = "sys/main.dol"
FRAMEWORK_MAP_PATH = "files/map/Final/Release/frameworkF.map"
REL_BASE_PATH = "files/rel/Final/Release"

if __name__ == "__main__":
    if os.name == "nt":
        DEFAULT_DOLPHIN_CONFIG_PATH = Path(os.getenv('APPDATA')) / "Dolphin Emulator"
    else:
        DEFAULT_DOLPHIN_CONFIG_PATH = Path("~/.local/share/dolphin-emu/").expanduser()

    # Validate default Dolphin config path
    if not DEFAULT_DOLPHIN_CONFIG_PATH.exists():
        logging.warning(f"Default Dolphin config path does not exist: {DEFAULT_DOLPHIN_CONFIG_PATH}. Maps will be placed in the specified --map directory.")

    def expanded_path(path_str: str) -> Path:
        """Expand user home directory in path string and return as Path object."""
        path = Path(path_str)
        path = path.expanduser()
        return path

    parser = argparse.ArgumentParser()

    parser.add_argument(
        "vanilla_iso_path",
        type=expanded_path,
        help="Path to a vanilla Twilight Princess ISO to use as a base.",
    )

    parser.add_argument(
        "output_iso_path",
        type=expanded_path,
        help="Path to put the modified ISO.",
    )

    parser.add_argument(
        "decomp_repo_path",
        type=expanded_path,
        help="Path to the root of the git repository containing the tp decompilation.",
    )

    parser.add_argument(
        "--map",
        type=expanded_path,
        default=DEFAULT_DOLPHIN_CONFIG_PATH / "Maps",
        help="Folder to place the symbol map for the modified ISO (defaults to Dolphin's maps directory).",
    )

    parser.add_argument(
        "--version",
        type=expanded_path,
        default="GZ2E01",
        help="Version of the game to build.",
    )

    args = parser.parse_args()
    decomp_build_path = args.decomp_repo_path / "build" / args.version

    # Validate input paths
    if not args.vanilla_iso_path.exists():
        logging.error(f"Vanilla ISO not found: {args.vanilla_iso_path}")
        sys.exit(1)
    if not args.vanilla_iso_path.is_file():
        logging.error(f"Vanilla ISO path is not a file: {args.vanilla_iso_path}")
        sys.exit(1)
    if not decomp_build_path.exists():
        logging.error(f"Decomp build directory not found: {decomp_build_path}")
        sys.exit(1)
    if not (decomp_build_path / "framework.dol").exists():
        logging.error(f"framework.dol not found in build directory: {decomp_build_path / 'framework.dol'}")
        sys.exit(1)
    if not (decomp_build_path / "framework.elf.MAP").exists():
        logging.error(f"framework.elf.MAP not found in build directory: {decomp_build_path / 'framework.elf.MAP'}")
        sys.exit(1)

    # Ensure output directory exists
    output_dir = args.output_iso_path.parent
    if not output_dir.exists():
        logging.info(f"Creating output directory: {output_dir}")
        output_dir.mkdir(parents=True, exist_ok=True)

    # Ensure map directory exists
    if not args.map.exists():
        logging.info(f"Creating map directory: {args.map}")
        args.map.mkdir(parents=True, exist_ok=True)

    logging.info(f"Starting rebuild with vanilla ISO: {args.vanilla_iso_path}, output: {args.output_iso_path}, decomp repo: {args.decomp_repo_path}")

    try:
        # Load the vanilla ISO and read its contents
        gcm = GCM(args.vanilla_iso_path)
        gcm.read_entire_disc()
        logging.info("Read entire disc from GCM")

        # Extract and read the RELS archive containing REL files
        rels_arc = RARC(gcm.read_file_data(RELS_ARC_PATH))
        rels_arc.read()

        # Process each REL file from the decomp build directory
        logging.info("Processing REL files from decomp build")
        for rel_name in os.listdir(decomp_build_path):
            rel_dir = decomp_build_path / rel_name
            if rel_dir.is_file():
                continue

            rel_file = rel_dir / f"{rel_name}.rel"
            if not rel_file.is_file():
                continue

            decomp_rel_path = rel_file
            with open(decomp_rel_path, "rb") as f:
                decomp_rel_data = BytesIO(f.read())
                rel_file_entry = rels_arc.get_file_entry(rel_name.lower() + ".rel")

                if rel_file_entry:
                    # Compress and update the REL in the archive
                    rel_file_entry.data = Yaz0.compress(decomp_rel_data)
                else:
                    # Add new REL file to the GCM
                    gcm_rel_file_path = f"{REL_BASE_PATH}/{rel_name}.rel"
                    if gcm_rel_file_path not in gcm.files_by_path:
                        logging.error(f"Invalid REL path: {gcm_rel_file_path}")
                        sys.exit(1)
                    gcm.changed_files[gcm_rel_file_path] = decomp_rel_data

        rels_arc.save_changes()
        gcm.changed_files[RELS_ARC_PATH] = rels_arc.data

        logging.info("Finished processing REL files")

        # Update the main DOL and map files
        with open(decomp_build_path / "framework.dol", "rb") as f:
            gcm.changed_files[MAIN_DOL_PATH] = BytesIO(f.read())

        with open(decomp_build_path / "framework.elf.MAP", "rb") as f:
            gcm.changed_files[FRAMEWORK_MAP_PATH] = BytesIO(f.read())

        logging.info("Updated framework.dol and map files")

        # Copy the map file to the specified location
        shutil.copy(decomp_build_path / "framework.elf.MAP", args.map)

        logging.info("Adding custom files")
        mod_assets_dir = Path('mod_assets')
        mod_assets_dir.mkdir(exist_ok=True)

        replace_paths, add_paths = gcm.collect_files_to_replace_and_add_from_disk(mod_assets_dir, gcm.dirs_by_path_lowercase["files"])

        for (file_path, gcm_file_path) in add_paths:
            if os.path.isfile(file_path):
                try:
                    with open(file_path, "rb") as f:
                        gcm.add_new_file(gcm_file_path, BytesIO(f.read()))
                        logging.info("Added custom file: %s" % gcm_file_path)
                except Exception as e:
                    logging.error(f"Couldn't add file: {file_path}")
            else:
                raise Exception("File appears to have been deleted or moved: %s" % gcm_file_path)

        # Export the modified ISO
        for _ in gcm.export_disc_to_iso_with_changed_files(args.output_iso_path):
            pass

        logging.info("Rebuild completed successfully")
    except Exception as e:
        logging.error(f"An error occurred during rebuild: {e}")
        sys.exit(1)

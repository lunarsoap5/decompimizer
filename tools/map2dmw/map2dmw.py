# This script converts a templatized watchfile into a DME Watchlist by reading in the framework.MAP file and dynamically identifying struct/function addresses.
# Example usage: python ./tools/map2dmw/map2dmw.py GZ2E01 tools/map2dmw/watchfile.jsonc -o ./tools/map2dmw/output.dmw
# NOTE: This code assumes it is being ran from the root project directory. 

import argparse
import json
import os
import re
import sys
from collections import defaultdict
 
LINE_RE = re.compile(
    r"^\s*([0-9a-fA-F]{8})\s+([0-9a-fA-F]{6,8})\s+([0-9a-fA-F]{8})\s+"
    r"([0-9a-fA-F]{8})\s+(\d+)\s+(\S+)\s*(\S*)\s*$"
)
OFFSET_RE = re.compile(r"\s*([+-])\s*(0[xX][0-9a-fA-F]+|\d+)\s*$")
RAW_HEX_RE = re.compile(r"^(?:0[xX])?([0-9a-fA-F]{8})$")
 
 
class SymbolTable:
    def __init__(self):
        self.by_name = defaultdict(list) 
 
    @classmethod
    def from_map(cls, path):
        table = cls()
        with open(path, "r", errors="replace") as f:
            for line in f:
                m = LINE_RE.match(line)
                if m:
                    _, _, vaddr, _, _, name, obj = m.groups()
                    table.by_name[name].append((int(vaddr, 16), obj))
        return table
 
    def lookup(self, name):
        # Return address or None if the symbol isn't in the map.
        obj = None
        cands = self.by_name.get(name)
        if cands is None and ":" in name:
            obj, bare = name.split(":", 1)
            cands = self.by_name.get(bare)
            name = bare
        if not cands:
            return None
        if obj:
            cands = [c for c in cands if c[1] == obj]
            if not cands:
                raise KeyError(f"symbol '{name}' not found in object '{obj}'")
        if len({c[0] for c in cands}) > 1:
            opts = ", ".join(f"{c[1]}@{c[0]:08X}" for c in cands)
            raise KeyError(f"symbol '{name}' is ambiguous ({opts}); use 'object.o:{name}'")
        return cands[0][0]
 
 
def eval_address(expr, table):
    rest = expr.strip()
    offset = 0
    while True:  
        m = OFFSET_RE.search(rest)
        if not m or m.start() == 0:
            break
        val = int(m.group(2), 0)
        offset += -val if m.group(1) == "-" else val
        rest = rest[: m.start()]
    base = rest.strip()
    if not base:
        raise ValueError(f"empty symbol in {expr!r}")
 
    addr = table.lookup(base)
    if addr is None:
        raw = RAW_HEX_RE.match(base)
        if raw:
            addr = int(raw.group(1), 16)
        else:
            raise KeyError(f"symbol not found in map: {base}")
    return (addr + offset) & 0xFFFFFFFF
 
 
def convert(node, table, fmt, errors, path=""):
    if isinstance(node, dict):
        out = {}
        label = node.get("label") or node.get("groupName") or ""
        here = f"{path}/{label}" if label else path
        for k, v in node.items():
            if k == "address" and isinstance(v, str):
                try:
                    out[k] = fmt(eval_address(v, table))
                except (KeyError, ValueError) as ex:
                    errors.append(f"{here}: {ex}")
                    out[k] = v
            else:
                out[k] = convert(v, table, fmt, errors, here)
        return out
    if isinstance(node, list):
        return [convert(v, table, fmt, errors, path) for v in node]
    return node
 
 
def main():
    ap = argparse.ArgumentParser(description="Resolve symbol addresses in a .dmw-style JSON using a region's .MAP file")
    ap.add_argument("region", help="region name; the map is read from <build-dir>/REGION/framework.elf.MAP")
    ap.add_argument("input", nargs="?", help="input JSON (dmw layout, symbol expressions in 'address')")
    ap.add_argument("-o", "--output", default="output.dmw")
    ap.add_argument("--build-dir", default="./build", help="build directory containing the region folders (default: .build)")
    ap.add_argument("--lookup", metavar="EXPR", help='evaluate one expression and print it, e.g. "__init_hardware + 0x43"')
    ap.add_argument("--lowercase", action="store_true", help="write addresses in lowercase hex (default: uppercase)")
    args = ap.parse_args()
 
    fmt = (lambda a: f"{a:08x}") if args.lowercase else (lambda a: f"{a:08X}")
 
    map_path = os.path.join(args.build_dir, args.region, "framework.elf.MAP")
    if not os.path.isfile(map_path):
        sys.exit(f"Map file not found: {map_path}")
    table = SymbolTable.from_map(map_path)
    if not table.by_name:
        sys.exit("No symbols parsed from map file - is the format right?")
 
    if args.lookup:
        try:
            print(fmt(eval_address(args.lookup, table)))
        except (KeyError, ValueError) as ex:
            sys.exit(str(ex))
        return
    if not args.input:
        ap.error("input JSON is required unless --lookup is used")
 
    with open(args.input) as f:
        data = json.load(f)
 
    errors = []
    result = convert(data, table, fmt, errors)
    if errors:
        print("Errors (nothing written):", file=sys.stderr)
        for e in errors:
            print("  " + e, file=sys.stderr)
        sys.exit(1)
 
    with open(args.output, "w") as f:
        json.dump(result, f, indent=4)
    print(f"Wrote {args.output} ({len(table.by_name)} symbols loaded)")
 
 
if __name__ == "__main__":
    main()
#ifndef D_S_D_S_NAME_H
#define D_S_D_S_NAME_H

#include "f_op/f_op_camera_mng.h"
#include "f_op/f_op_scene.h"
#include "d/d_file_select.h"

class dSn_HIO_c {
public:
    dSn_HIO_c();
    virtual ~dSn_HIO_c() {}

    /* 0x4 */ s8 field_0x4;
    /* 0x5 */ u8 mFileSelWaitTime;
};

class dScnName_camera_c : public camera_process_class {
public:
    dScnName_camera_c() { field_0x22f = 84; }
    virtual ~dScnName_camera_c() {}
};

// remove later and use header instead
class dBrightCheck_c {
public:
    dBrightCheck_c(JKRArchive*);
    virtual ~dBrightCheck_c();
    void _move();
    void _draw();

    bool isEnd() { return mEnd; }

    u8 field_0x4[0x15];
    /* 0x19 */ bool mEnd;
};

class dScnName_c : public scene_class {
public:
    dScnName_c() {}

    s32 create();
    void setView();
    s32 execute();
    s32 draw();
    ~dScnName_c();
    void FileSelectOpen();
    void FileSelectMain();
    void FileSelectMainNormal();
    void FileSelectClose();
    void brightCheckOpen();
    void brightCheck();
    void changeGameScene();

    #if VERSION == VERSION_GCN_PAL
    void bmg_data_set();
    void tex_data_set();
    #endif

private:
    #if VERSION == VERSION_GCN_PAL
    u8 field_0x1c4_pal[0x28];
    #endif
    /* 0x1C4 */ request_of_phase_process_class field_0x1c4;
    /* 0x1CC */ JKRExpHeap* mHeap;
    /* 0x1D0 */ JKRExpHeap* field_0x1d0;
    /* 0x1D4 */ dScnName_camera_c mCamera;
    /* 0x414 */ dFile_select_c* dFs_c;
    /* 0x418 */ dBrightCheck_c* mBrightCheck;
    /* 0x41C */ u8 field_0x41c;
    /* 0x41D */ u8 field_0x41d;
    /* 0x41E */ u8 field_0x41e;
    /* 0x41F */ u8 field_0x41f;
    /* 0x420 */ u8 field_0x420;
};

#endif /* D_S_D_S_NAME_H */

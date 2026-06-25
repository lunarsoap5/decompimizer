class ReturnPlace
{
    public:
    ReturnPlace() {}
    ~ReturnPlace() {}

    u8 getStageIDX() const { return stageIDX; }
    u8 getRoomNo() const { return roomNo; }
    u8 getPoint() const { return point; }
    s8 getLayer() const { return layer; }

    private:
    u8 stageIDX;
    u8 roomNo;
    u8 point;
    s8 layer;
};

class ReturnPlaceSection
{
    public:
    ReturnPlaceSection() {}
    ~ReturnPlaceSection() {}

    const ReturnPlace* getReturnPlace(u8 stageIDX, s8 roomNo, s8 point, s8 layer) const;

    private:
    class Comparison
    {
        public:
        Comparison() {}
        ~Comparison() {}

        u8 stageIDX;
        s8 roomNo;
        s8 point;
        s8 layer;
    };

    /* 0x00 */ u16 numComparisons;
    /* 0x02 */ u16 matchIndexOffset;
    /* 0x04 */ u16 comparisonsOffset;
    /* 0x06 */ u16 numReturnPlaces;
    /* 0x08 */ u16 returnPlacesOffset;
};

﻿namespace NGen {

    public enum CharType { comment, declare, openList, closeList, listSeparator, proxy, header, noSepBefore, noSepAfter };

    public enum PickType { random, shuffle, cycle, weighted };

    public enum RepeatType { normal, uniform, @fixed, weighted };

}

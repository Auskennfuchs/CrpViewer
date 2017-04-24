namespace CrpExtractor.Parsers
{
    static class ImgParser
    {
        public static byte[] parseImage(CrpReader reader, long fileSize)
        {
            bool forceLinearFlag = reader.ReadBoolean();
            uint imgLength = reader.ReadUInt32();
            var imgData = reader.ReadBytes((int)imgLength);
            return imgData;
        }
    }
}

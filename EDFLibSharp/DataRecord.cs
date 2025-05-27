namespace EDFLibSharp
{
    public class DataRecord(uint sampleRate, uint index)
    {
        public uint SampleRate { get; } = sampleRate;
        public uint Index { get; } = index;
        public uint StartRecord { get; set; }
        public uint ReadCount { get; set; }
        public uint Length => SampleRate * ReadCount;
        public List<double> Buffer { get; } = [];

        public void Clear()
        {
            Buffer.Clear();
        }

        public void Add(double value)
        {
            Buffer.Add(value);
        }
    }
}


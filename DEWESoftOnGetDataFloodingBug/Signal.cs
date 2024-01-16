namespace DEWESoftOnGetDataFloodingBug
{
    public class Signal
    {
        public string Name { get; set; }
        public int DBPos { get; set; }

        public Signal(string name, int initdbpos = 0)
        {
            Name = name;
            DBPos = initdbpos;
        }
    }
}

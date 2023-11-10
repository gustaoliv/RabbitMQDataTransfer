namespace Types
{
    public class CodeObject
    {
        public int MT_CODIGO                { get; set; }
        public string MT_IMPLEMENTACAO      { get; set; }
        public string MT_FQN                { get; set; }
        public int CF_CODIGO                { get; set; }
        public string MT_PACOTE             { get; set; }
        public string MT_CLASSE             { get; set; }
        public string MT_METODO             { get; set; }
        public string MT_METODOSPLITCAMEL   { get; set; }
        public bool ALREADY_EXPORTED        { get; set; }
    }
}

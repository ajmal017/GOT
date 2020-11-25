namespace GOT.Logic.Models
{
    public struct Account
    {
        /// <summary>
        ///     Account of data providers
        /// </summary>
        /// <param name="name">account</param>
        public Account(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
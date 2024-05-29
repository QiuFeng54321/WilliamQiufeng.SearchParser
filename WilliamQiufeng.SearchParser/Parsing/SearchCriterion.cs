using WilliamQiufeng.SearchParser.Tokenizing;

namespace WilliamQiufeng.SearchParser.Parsing
{
    public class SearchCriterion : Nonterminal
    {
        public SearchCriterion(TokenRange tokenRange, Token key, Token @operator, Token value, bool invert) :
            base(tokenRange)
        {
            Key = key;
            Operator = @operator;
            Value = value;
            Invert = invert;
        }

        public Token Key { get; set; }
        public Token Operator { get; set; }
        public Token Value { get; set; }
        public bool Invert { get; set; }
    }
}
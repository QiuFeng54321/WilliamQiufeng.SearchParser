namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class IntegerState : ITokenizerState
    {
        private int _currentInteger;

        public ITokenizerState Process(Tokenizer tokenizer)
        {
            var lookahead = tokenizer.Lookahead();

            switch (lookahead)
            {
                case '\0':
                case ' ':
                    tokenizer.EmitToken(TokenKind.Integer, _currentInteger);
                    return EmptyState.State;
                case '%':
                    tokenizer.Advance();
                    tokenizer.EmitToken(TokenKind.Percentage, _currentInteger);
                    return EmptyState.State;
                case '.':
                    tokenizer.Advance();
                    return new RealState();
            }

            if (TimeSpanState.Trie.TryNext(lookahead, out _) || lookahead == ':')
            {
                return new TimeSpanState(_currentInteger);
            }

            if (lookahead is < '0' or > '9')
            {
                return tokenizer.KeywordTrie.TryNext(tokenizer.BufferContent.Span, out var subTrie)
                    ? new KeyState(subTrie)
                    : PlainTextState.State;
            }

            tokenizer.Advance();
            _currentInteger = _currentInteger * 10 + lookahead - '0';
            return this;
        }
    }
}
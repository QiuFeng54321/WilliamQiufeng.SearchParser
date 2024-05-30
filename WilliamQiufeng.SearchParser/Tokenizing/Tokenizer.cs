using System;
using System.Collections;
using System.Collections.Generic;

namespace WilliamQiufeng.SearchParser.Tokenizing
{
    public class Tokenizer : IEnumerable<Token>
    {
        private readonly Queue<Token> _emittingTokens = new();
        internal readonly string Content;
        private ITokenizerState _currentState;
        private int _currentTokenEndPos = -1;
        private int _currentTokenStartPos;
        private int _lookaheadPos;

        /// <summary>
        ///     Whether to recognize keys or enums, or both, or ignore anything and treat them as plain texts
        /// </summary>
        internal KeyEnumResolveMode KeyEnumResolveMode = KeyEnumResolveMode.Both;

        public Tokenizer(string content)
        {
            Content = content;
            _currentState = EmptyState.State;
        }

        public Trie<object> KeywordTrie { get; } = new();
        public Trie<object> EnumTrie { get; } = new();

        internal ReadOnlyMemory<char> BufferContent
        {
            get
            {
                var length = _currentTokenEndPos - _currentTokenStartPos + 1;
                var segment = _currentTokenStartPos < Content.Length
                    ? Content.AsMemory().Slice(_currentTokenStartPos, length)
                    : new ReadOnlyMemory<char>();
                return segment;
            }
        }

        public IEnumerator<Token> GetEnumerator()
        {
            Token nextToken;
            do
            {
                nextToken = NextToken();
                yield return nextToken;
            } while (nextToken.Kind != TokenKind.End);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal char Lookahead()
        {
            return _lookaheadPos < Content.Length ? Content[_lookaheadPos] : '\0';
        }

        internal void Advance()
        {
            _currentTokenEndPos = _lookaheadPos;
            if (_lookaheadPos < Content.Length)
                _lookaheadPos++;
        }

        internal Token GenerateToken(TokenKind kind, object? content = default, bool isCompleteEnum = false)
        {
            var token = new Token(kind, BufferContent, _currentTokenStartPos, content);
            token.IsCompleteEnum = isCompleteEnum;
            DiscardBuffer();
            return token;
        }

        internal void DiscardBuffer()
        {
            _currentTokenStartPos = _lookaheadPos;
            _currentTokenEndPos = _currentTokenStartPos;
        }

        internal void EmitToken(TokenKind kind, object? content = default, bool isCompleteEnum = false)
        {
            EmitToken(GenerateToken(kind, content, isCompleteEnum));
        }

        internal void EmitToken(Token token)
        {
            _emittingTokens.Enqueue(token);
        }

        private bool Next()
        {
            if (_currentState is EndState)
                return false;
            _currentState = _currentState.Process(this);
            return true;
        }

        /// <summary>
        ///     Continuously call <see cref="Next"/> until a token is emitted in the queue
        /// </summary>
        /// <returns></returns>
        public Token NextToken()
        {
            do
            {
                if (_emittingTokens.TryDequeue(out var nextToken))
                    return nextToken;
            } while (Next());

            return GenerateToken(TokenKind.End);
        }

        public static IEnumerable<Token> TokenizeAsPlainTextTokens(ReadOnlyMemory<char> view, int offset = 0)
        {
            var sliceStart = 0;
            var sliceEnd = -1;

            while (sliceEnd < view.Length)
            {
                if (sliceEnd == view.Length - 1 || view.Span[sliceEnd + 1] == ' ')
                {
                    var length = sliceEnd - sliceStart + 1;
                    if (length > 0)
                    {
                        var segment = view.Slice(sliceStart, length);
                        yield return new Token(TokenKind.PlainText, segment, sliceStart + offset, segment.ToString());
                    }

                    sliceStart = sliceEnd + 2;
                }

                sliceEnd++;
            }
        }
    }
}
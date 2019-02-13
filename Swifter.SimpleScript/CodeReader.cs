using Swifter.Tools;
using System.Runtime.CompilerServices;

namespace Swifter.SimpleScript
{
    sealed unsafe class CodeReader
    {
        public static readonly char[] NameBeginChars =
        {
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '_', '$'
        };

        public static readonly char[] NumberBeginChars =
        {
            '0','1','2','3','4','5','6','7','8','9','+','-','.'
        };

        public static readonly char[] AnyBeginChars =
        {
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            '_', '$','~','!','@','#','$','^','(','{','[',
            '0','1','2','3','4','5','6','7','8','9','+','-','.'
        };

        public static bool IsBlank(char c)
        {
            switch (c)
            {
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                    return true;
                default:
                    return false;
            }
        }

        readonly IdCache<ProcessCacheItem> ProcessCaches;

        readonly IdCache<MethodCacheItem> MethodCaches;

        const long MethodCacheKeys_ReadName = 0x100000000;
        const long MethodCacheKeys_ReadNumber = 0x200000000;
        const long MethodCacheKeys_ReadOperator = 0x400000000;

        readonly char* chars;

        int index;
        readonly int end;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public CodeReader(char* chars, int start, int length)
        {
            this.chars = chars;

            index = start;

            end = start + length;

            ProcessCaches = new IdCache<ProcessCacheItem>();
            MethodCaches = new IdCache<MethodCacheItem>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public CodeReader(char* chars, int start, int length, CodeReader baseReader )
        {
            this.chars = chars;

            index = start;

            end = start + length;

            ProcessCaches = baseReader.ProcessCaches;
            MethodCaches = baseReader.MethodCaches;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IProcess Cache(Priorities priority)
        {
            if (ProcessCaches.TryGetValue(index, out var item) && item.Priority <= priority)
            {
                index = item.Index;

                return item.Process;
            }

            return null;
        }

        public CodeReaderBackup Backup() => new CodeReaderBackup(this, index);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool InternalReadName(out string name)
        {
            char c;

            name = null;

            if (SkipBlank() && (c = chars[index]) >= 0 && ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == '_') || (c == '$')))
            {
                var start = index;

                do
                {
                    ++index;
                } while (index < end && (c = chars[index]) >= 0 && ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || (c == '_') || (c == '$')));

                name = new string(chars, start, index - start);

                return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool InternalReadNumber(out string text)
        {
            text = null;

            if (CurrentChar(out var c) && ((c >= '0' && c <= '9') || (c == '.') || (c == '+') || (c == '-')))
            {
                var start = index;

                if (c == '+' || c == '-' || c == '.')
                {
                    if (++index >= end)
                    {
                        goto False;
                    }

                    c = chars[index];
                }

                if (c == '0')
                {
                    if (++index >= end)
                    {
                        goto True;
                    }

                    c = chars[index];

                    switch (c)
                    {
                        case 'e':
                        case 'E':
                        case 'x':
                        case 'X':
                        case 'b':
                        case 'B':

                            if (++index >= end)
                            {
                                goto False;
                            }

                            c = chars[index];

                            break;
                    }
                }

            Loop:

                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '.'))
                {
                    ++index;

                    c = chars[index];

                    goto Loop;
                }

                switch (c)
                {
                    case 'd':
                    case 'D':
                    case 'f':
                    case 'F':
                    case 'l':
                    case 'L':
                    case 'm':
                    case 'M':
                        ++index;
                        break;
                }


                True:

                text = new string(chars, start, index - start);

                return true;
            }

            False:
            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool InternalReadOperator(out string text)
        {
            text = null;

            SkipBlank();

            var start = index;

            while (index < end)
            {
                var c = chars[index];

                switch (c)
                {
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '~':
                    case '!':
                    case '%':
                    case '^':
                    case '&':
                    case '=':
                    case '|':
                    case '<':
                    case '>':
                    case '?':
                    case ':':
                        ++index;
                        continue;
                }

                if (index != start)
                {
                    text = new string(chars, start, index - start);

                    return true;
                }

                return false;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadName(out string name)
        {
            var cache = MethodCaches.GetOrAdd(MethodCacheKeys_ReadName | (uint)index, 
                key => new MethodCacheItem() { Result = InternalReadName(out var value), Value = value, Index = index});

            index = cache.Index;
            name = cache.Value;
            return cache.Result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadNumber(out string name)
        {
            var cache = MethodCaches.GetOrAdd(MethodCacheKeys_ReadName | (uint)index,
                key => new MethodCacheItem() { Result = InternalReadNumber(out var value), Value = value, Index = index });

            index = cache.Index;
            name = cache.Value;
            return cache.Result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadOperator(out string name)
        {
            var cache = MethodCaches.GetOrAdd(MethodCacheKeys_ReadName | (uint)index,
                key => new MethodCacheItem() { Result = InternalReadOperator(out var value), Value = value, Index = index });

            index = cache.Index;
            name = cache.Value;
            return cache.Result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Next() => (++index) <= end;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadChar(out char c) => CurrentChar(out c) && Next();

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool CurrentChar(out char c) => (SkipBlank() ? (c = chars[index]) : (c = '\0')) >= 0;
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool SkipBlank()
        {
            Loop:

            if (index < end)
            {
                if (IsBlank(chars[index]))
                {
                    ++index;

                    goto Loop;
                }

                return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool SkipEnd()
        {
            if (CurrentChar(out var c))
            {
                if (c == ';')
                {
                    Next();

                    return true;
                }
            }

            return false;
        }

        class ProcessCacheItem
        {
            public IProcess Process;
            public int Index;
            public Priorities Priority;
        }

        class MethodCacheItem
        {
            public string Value;
            public bool Result;
            public int Index;
        }

        public struct CodeReaderBackup
        {
            readonly CodeReader Reader;

            public readonly int Index;

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public CodeReaderBackup(CodeReader reader, int index)
            {
                Index = index;
                Reader = reader;
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public bool Cache(Priorities priority, IProcess process)
            {
                var cache = Reader.ProcessCaches.LockGetOrAdd(Index, Id => new ProcessCacheItem()
                {
                    Priority = priority
                });

                if (cache.Priority <= priority)
                {
                    cache.Priority = priority;
                    cache.Process = process;
                    cache.Index = Reader.index;

                    return true;
                }

                return false;
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public void Restore()
            {
                Reader.index = Index;
            }
        }
    }
}

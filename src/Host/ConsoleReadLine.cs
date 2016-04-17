// <copyright file="readline.cs" company="Microsoft Corporation">
// Copyright (c) 2009 Microsoft Corporation. All rights reserved.
// </copyright>
// DISCLAIMER OF WARRANTY: The software is licensed “as-is.” You 
// bear the risk of using it. Microsoft gives no express warranties, 
// guarantees or conditions. You may have additional consumer rights 
// under your local laws which this agreement cannot change. To the extent 
// permitted under your local laws, Microsoft excludes the implied warranties 
// of merchantability, fitness for a particular purpose and non-infringement.

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text;

namespace PSExt.Host
{
	/// <summary>
    /// This class is used to read a PowerShell command line, colorizing the 
    /// text as it is entered. Tokens are determined using the PSParser.Tokenize
    /// class.
    /// </summary>
    public class ConsoleReadLine
    {
        /// <summary>
        /// The buffer used to edit.
        /// </summary>
        private StringBuilder buffer = new StringBuilder();

        /// <summary>
        /// The position of the cursor within the buffer.
        /// </summary>
        private int current;

        /// <summary>
        /// The count of characters in buffer rendered.
        /// </summary>
        private int rendered;

        /// <summary>
        /// Store the anchor and handle cursor movement
        /// </summary>
        private Cursor cursor;

        /// <summary>
        /// The array of colors for tokens, indexed by PSTokenType
        /// </summary>
        private ConsoleColor[] tokenColors;

        /// <summary>
        /// We don't pick different colors for every token, those tokens
        /// use this default.
        /// </summary>
        private ConsoleColor defaultColor = Console.ForegroundColor;

        /// <summary>
        /// Initializes a new instance of the ConsoleReadLine class.
        /// </summary>
        public ConsoleReadLine()
        {
            tokenColors = new ConsoleColor[]
            {
                defaultColor,       // Unknown
                ConsoleColor.Yellow,     // Command
                ConsoleColor.Green,      // CommandParameter
                ConsoleColor.Cyan,       // CommandArgument
                ConsoleColor.Cyan,       // Number
                ConsoleColor.Cyan,       // String
                ConsoleColor.Green,      // Variable
                defaultColor,            // Member
                defaultColor,            // LoopLabel
                ConsoleColor.DarkYellow, // Attribute
                ConsoleColor.DarkYellow, // Type
                ConsoleColor.DarkCyan,   // Operator
                defaultColor,            // GroupStart
                defaultColor,            // GroupEnd
                ConsoleColor.Magenta,    // Keyword
                ConsoleColor.Red,        // Comment
                ConsoleColor.DarkCyan,   // StatementSeparator
                defaultColor,            // NewLine
                defaultColor,            // LineContinuation
                defaultColor,            // Position            
            };
        }

        /// <summary>
        /// Read a line of text, colorizing while typing.
        /// </summary>
        /// <returns>The command line read</returns>
        public string Read()
        {
            Initialize();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key)
                {
                case ConsoleKey.Backspace:
                    OnBackspace();
                    break;
                case ConsoleKey.Delete:
                    OnDelete();
                    break;
                case ConsoleKey.Enter:
                    return OnEnter();
                case ConsoleKey.RightArrow:
                    OnRight(key.Modifiers);
                    break;
                case ConsoleKey.LeftArrow:
                    OnLeft(key.Modifiers);
                    break;
                case ConsoleKey.Escape:
                    OnEscape();
                    break;
                case ConsoleKey.Home:
                    OnHome();
                    break;
                case ConsoleKey.End:
                    OnEnd();
                    break;
                case ConsoleKey.UpArrow:
                case ConsoleKey.DownArrow:
                case ConsoleKey.LeftWindows:
                case ConsoleKey.RightWindows:
                    // ignore these
                    continue;

                default:
                    if (key.KeyChar == '\x0D')
                    {
                        goto case ConsoleKey.Enter;      // Ctrl-M
                    }

                    if (key.KeyChar == '\x08')
                    {
                        goto case ConsoleKey.Backspace;  // Ctrl-H
                    }

                    Insert(key);
                    break;
                }
            }
        }

        /// <summary>
        /// Initializes the buffer.
        /// </summary>
        private void Initialize()
        {
            buffer.Length = 0;
            current = 0;
            rendered = 0;
            cursor = new Cursor();
        }

        /// <summary>
        /// Inserts a key.
        /// </summary>
        /// <param name="key">The key to insert.</param>
        private void Insert(ConsoleKeyInfo key)
        {
            buffer.Insert(current, key.KeyChar);
            current++;
            Render();
        }

        /// <summary>
        /// The End key was enetered..
        /// </summary>
        private void OnEnd()
        {
            current = buffer.Length;
            cursor.Place(rendered);
        }

        /// <summary>
        /// The Home key was eneterd.
        /// </summary>
        private void OnHome()
        {
            current = 0;
            cursor.Reset();
        }

        /// <summary>
        /// The Escape key was enetered.
        /// </summary>
        private void OnEscape()
        {
            buffer.Length = 0;
            current = 0;
            Render();
        }

        /// <summary>
        /// Moves to the left of the cursor postion.
        /// </summary>
        /// <param name="consoleModifiers">Enumeration for Alt, Control, 
        /// and Shift keys.</param>
        private void OnLeft(ConsoleModifiers consoleModifiers)
        {
            if ((consoleModifiers & ConsoleModifiers.Control) != 0)
            {
                // Move back to the start of the previous word.
                if (buffer.Length > 0 && current != 0)
                {
                    bool nonLetter = IsSeperator(buffer[current - 1]);
                    while (current > 0 && (current - 1 < buffer.Length))
                    {
                        MoveLeft();

                        if (IsSeperator(buffer[current]) != nonLetter)
                        {
                            if (!nonLetter)
                            {
                                MoveRight();
                                break;
                            }

                            nonLetter = false;
                        }
                    }
                }
            }
            else
            {
                MoveLeft();
            }
        }

        /// <summary>
        /// Determines if a character is a seperator.
        /// </summary>
        /// <param name="ch">Character to investigate.</param>
        /// <returns>A value that incicates whether the character 
        /// is a seperator.</returns>
        private static bool IsSeperator(char ch)
        {
            return !Char.IsLetter(ch);
        }

        /// <summary>
        /// Moves to what is to the right of the cursor position.
        /// </summary>
        /// <param name="consoleModifiers">Enumeration for Alt, Control, 
        /// and Shift keys.</param>
        private void OnRight(ConsoleModifiers consoleModifiers)
        {
            if ((consoleModifiers & ConsoleModifiers.Control) != 0)
            {
                // Move to the next word.
                if (buffer.Length != 0 && current < buffer.Length)
                {
                    bool nonLetter = IsSeperator(buffer[current]);
                    while (current < buffer.Length)
                    {
                        MoveRight();

                        if (current == buffer.Length)
                        {
                            break;
                        }

                        if (IsSeperator(buffer[current]) != nonLetter)
                        {
                            if (nonLetter)
                            {
                                break;
                            }

                            nonLetter = true;
                        }
                    }
                }
            }
            else
            {
                MoveRight();
            }
        }

        /// <summary>
        /// Moves the cursor one character to the right.
        /// </summary>
        private void MoveRight()
        {
            if (current < buffer.Length)
            {
                char c = buffer[current];
                current++;
                Cursor.Move(1);
            }
        }

        /// <summary>
        /// Moves the cursor one character to the left.
        /// </summary>
        private void MoveLeft()
        {
            if (current > 0 && (current - 1 < buffer.Length))
            {
                current--;
                char c = buffer[current];
                Cursor.Move(-1);
            }
        }

        /// <summary>
        /// The Enter key was entered.
        /// </summary>
        /// <returns>A newline character.</returns>
        private string OnEnter()
        {
            Console.Out.Write("\n");
            return buffer.ToString();
        }

        /// <summary>
        /// The delete key was entered.
        /// </summary>
        private void OnDelete()
        {
            if (buffer.Length > 0 && current < buffer.Length)
            {
                buffer.Remove(current, 1);
                Render();
            }
        }

        /// <summary>
        /// The Backspace key was entered.
        /// </summary>
        private void OnBackspace()
        {
            if (buffer.Length > 0 && current > 0)
            {
                buffer.Remove(current - 1, 1);
                current--;
                Render();
            }
        }

        /// <summary>
        /// Displays the line.
        /// </summary>
        private void Render()
        {
            string text = buffer.ToString();

            // The PowerShell tokenizer is used to decide how to colorize
            // the input.  Any errors in the input are returned in 'errors',
            // but we won't be looking at those here.
            Collection<PSParseError> errors = null;
            Collection<PSToken> tokens = PSParser.Tokenize(text, out errors);

            if (tokens.Count > 0)
            {
                // We can skip rendering tokens that end before the cursor.
                int i;
                for (i = 0; i < tokens.Count; ++i)
                {
                    if (current >= tokens[i].Start)
                    {
                        break;
                    }
                }

                // Place the cursor at the start of the first token to render.  The
                // last edit may require changes to the colorization of characters
                // preceding the cursor.
                cursor.Place(tokens[i].Start);

                for (; i < tokens.Count; ++i)
                {
                    // Write out the token.  We don't use tokens[i].Content, instead we
                    // use the actual text from our input because the content sometimes
                    // excludes part of the token, e.g. the quote characters of a string.
                    Console.ForegroundColor = tokenColors[(int)tokens[i].Type];
                    Console.Out.Write(text.Substring(tokens[i].Start, tokens[i].Length));

                    // Whitespace doesn't show up in the array of tokens.  Write it out here.
                    if (i != (tokens.Count - 1))
                    {
                        Console.ForegroundColor = defaultColor;
                        for (int j = (tokens[i].Start + tokens[i].Length); j < tokens[i + 1].Start; ++j)
                        {
                            Console.Out.Write(text[j]);
                        }
                    }
                }

                // It's possible there is text left over to output.  This happens when there is
                // some error during tokenization, e.g. an string literal missing a closing quote.
                Console.ForegroundColor = defaultColor;
                for (int j = tokens[i - 1].Start + tokens[i - 1].Length; j < text.Length; ++j)
                {
                    Console.Out.Write(text[j]);
                }
            }
            else
            {
                // If tokenization completely failed, just redraw the whole line.  This
                // happens most frequently when the first token is incomplete, like a string
                // literal missing a closing quote.
                cursor.Reset();
                Console.Out.Write(text);
            }

            // If characters were deleted, we must write over previously written characters
            if (text.Length < rendered)
            {
                Console.Out.Write(new string(' ', rendered - text.Length));
            }

            rendered = text.Length;
            cursor.Place(current);
        }

        /// <summary>
        /// A helper class for maintaining the cursor while editing the command line.
        /// </summary>
        internal class Cursor
        {
            /// <summary>
            /// The top anchor for reposition the cursor.
            /// </summary>
            private int anchorTop;

            /// <summary>
            /// The left anchor for repositioning the cursor.
            /// </summary>
            private int anchorLeft;

            /// <summary>
            /// Initializes a new instance of the Cursor class.
            /// </summary>
            public Cursor()
            {
                anchorTop = Console.CursorTop;
                anchorLeft = Console.CursorLeft;
            }

            /// <summary>
            /// Moves the cursor.
            /// </summary>
            /// <param name="delta">The number of characters to move.</param>
            internal static void Move(int delta)
            {
                int position = Console.CursorTop * Console.BufferWidth + Console.CursorLeft + delta;

                Console.CursorLeft = position % Console.BufferWidth;
                Console.CursorTop = position / Console.BufferWidth;
            }

            /// <summary>
            /// Resets the cursor position.
            /// </summary>
            internal void Reset()
            {
                Console.CursorTop = anchorTop;
                Console.CursorLeft = anchorLeft;
            }

            /// <summary>
            /// Moves the cursor to a specific position.
            /// </summary>
            /// <param name="position">The new position.</param>
            internal void Place(int position)
            {
                Console.CursorLeft = (anchorLeft + position) % Console.BufferWidth;
                int cursorTop = anchorTop + (anchorLeft + position) / Console.BufferWidth;
                if (cursorTop >= Console.BufferHeight)
                {
                    anchorTop -= cursorTop - Console.BufferHeight + 1;
                    cursorTop = Console.BufferHeight - 1;
                }

                Console.CursorTop = cursorTop;
            }
        } // End Cursor
    }
}

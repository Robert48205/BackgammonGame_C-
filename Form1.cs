using BackgammonGame.Classes;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Windows.Forms;

namespace BackgammonGame
{
    public partial class Form1 : Form
    {
        #region Class Variables and Components
        
        // UI components for the game board
        private TableLayoutPanel boardPanel = null!;
        private Panel centerBar = null!;
        private PictureBox[,] triangles = null!;
        private List<PictureBox>[,] piecesOnBoard = null!;

        // UI components for dice and controls
        private Button rollButton = null!;
        private Label die1Label = null!;
        private Label die2Label = null!;
        private Panel dicePanel = null!;
        
        // UI components for OUT areas
        private Panel whiteOutPanel = null!;
        private Panel blackOutPanel = null!;
        private Label whiteOutLabel = null!;
        private Label blackOutLabel = null!;

        // Lists to track pieces in different areas
        private List<PictureBox> whiteOutPieces = new List<PictureBox>();
        private List<PictureBox> blackOutPieces = new List<PictureBox>();
        private List<PictureBox> whiteBarPieces = new List<PictureBox>();
        private List<PictureBox> blackBarPieces = new List<PictureBox>();

        // Game logic components
        private Board gameBoard = null!;
        private Piece? selectedPiece;
        private Dice dice = null!;
        private int die1Value;
        private int die2Value;
        private List<int> usedDiceValues = new List<int>();
        private List<int> availableMoves = new List<int>();

        // Player management
        private Player player1 = null!;
        private Player player2 = null!;
        private Player currentPlayer = null!;
        private bool diceRolled = false;

        private Point? selectedTriangle = null;

        // Special position constants
        private const int WHITE_BAR_POSITION = 25;
        private const int BLACK_BAR_POSITION = -2;
        private const int WHITE_OUT_POSITION = 24;
        private const int BLACK_OUT_POSITION = -1;

        #endregion
        
        // Main form constructor
        public Form1()
        {
            InitializeComponent();
            SetupControls();
            InitializeGame();
        }

        #region Drawing Methods, Grid and Label Management
        
        // Initializes all UI controls and layout
        private void SetupControls()
        {
            // Set window properties
            this.WindowState = FormWindowState.Maximized;
            this.MinimumSize = new Size(1200, 700);
            this.BackColor = Color.SandyBrown;

            // Create main layout with 3 columns
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 1,
                ColumnCount = 3,
                Padding = new Padding(20)
            };

            // Configure column widths (15% - 70% - 15%)
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));

            // Setup the three main areas
            SetupBlackOutArea(mainLayout);
            SetupBoardArea(mainLayout);
            SetupRightArea(mainLayout);

            this.Controls.Add(mainLayout);
        }

        // Creates the black OUT area on the left side
        private void SetupBlackOutArea(TableLayoutPanel mainLayout)
        {
            // Create panel for black pieces that have been borne off
            blackOutPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                AutoScroll = true,
                Cursor = Cursors.Hand
            };
            
            // Setup click event for bearing off
            blackOutPanel.Click += BlackOut_Click;
            
            // Highlight on mouse enter if valid move
            blackOutPanel.MouseEnter += (s, e) =>
            {
                if (selectedPiece != null && selectedPiece.Color == "Black" && gameBoard.AllBlackHome)
                {
                    blackOutPanel.BackColor = Color.FromArgb(80, 80, 80);
                }
            };
            
            // Remove highlight on mouse leave
            blackOutPanel.MouseLeave += (s, e) =>
            {
                blackOutPanel.BackColor = Color.FromArgb(60, 60, 60);
            };

            // Create label showing black OUT status
            blackOutLabel = new Label
            {
                Text = "BLACK OUT\n(Click to bear off)\n(0)",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Height = 80,
                BackColor = Color.Black
            };
            blackOutPanel.Controls.Add(blackOutLabel);

            mainLayout.Controls.Add(blackOutPanel, 0, 0);
        }

        // Creates the main game board area in the center
        private void SetupBoardArea(TableLayoutPanel mainLayout)
        {
            // Container for the board
            Panel boardContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.SaddleBrown,
                Padding = new Padding(15, 12, 15, 12)
            };

            // Create 2x13 grid (2 rows, 12 triangle columns + 1 center bar)
            boardPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 13,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };

            // Setup row heights (50% each)
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            boardPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Setup column widths (12 equal columns + 1 fixed center bar)
            for (int i = 0; i < 13; i++)
            {
                if (i == 6)
                    boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
                else
                    boardPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 12));
            }

            // Initialize arrays for triangles and pieces
            triangles = new PictureBox[2, 12];
            piecesOnBoard = new List<PictureBox>[2, 12];

            // Create all triangle positions
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 12; col++)
                {
                    // Adjust column index to account for center bar
                    int gridCol = col < 6 ? col : col + 1;
                    
                    // Create triangle PictureBox
                    triangles[row, col] = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        BackColor = Color.Transparent,
                        Tag = new Point(row, col),
                        SizeMode = PictureBoxSizeMode.Normal,
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0)
                    };

                    // Setup event handlers
                    triangles[row, col].Paint += Triangle_Paint;
                    triangles[row, col].Click += Triangle_Click;
                    
                    // Capture loop variables for lambda
                    int rr = row, cc = col;
                    triangles[row, col].SizeChanged += (s, e) => RelayoutTriangle(rr, cc);

                    // Initialize piece list for this triangle
                    piecesOnBoard[row, col] = new List<PictureBox>();
                    boardPanel.Controls.Add(triangles[row, col], gridCol, row);
                }
            }

            // Create center bar panel
            centerBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.SaddleBrown,
                Margin = new Padding(0)
            };
            centerBar.Paint += CenterBar_Paint;
            centerBar.Click += CenterBar_Click;

            // Span center bar across both rows
            boardPanel.SetRowSpan(centerBar, 2);
            boardPanel.Controls.Add(centerBar, 6, 0);

            boardContainer.Controls.Add(boardPanel);
            mainLayout.Controls.Add(boardContainer, 1, 0);
        }

        // Creates the right panel with white OUT and dice controls
        private void SetupRightArea(TableLayoutPanel mainLayout)
        {
            // Create 2-row panel for white OUT and dice
            TableLayoutPanel rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // Create white OUT panel
            whiteOutPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                AutoScroll = true,
                Cursor = Cursors.Hand
            };
            
            // Setup click event for bearing off
            whiteOutPanel.Click += WhiteOut_Click;
            
            // Highlight on mouse enter if valid move
            whiteOutPanel.MouseEnter += (s, e) =>
            {
                if (selectedPiece != null && selectedPiece.Color == "White" && gameBoard.AllWhiteHome)
                {
                    whiteOutPanel.BackColor = Color.FromArgb(220, 220, 220);
                }
            };
            
            // Remove highlight on mouse leave
            whiteOutPanel.MouseLeave += (s, e) =>
            {
                whiteOutPanel.BackColor = Color.FromArgb(240, 240, 240);
            };

            // Create label showing white OUT status
            whiteOutLabel = new Label
            {
                Text = "WHITE OUT\n(Click to bear off)\n(0)",
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.Black,
                Height = 80,
                BackColor = Color.White
            };
            whiteOutPanel.Controls.Add(whiteOutLabel);

            // Create dice panel
            dicePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(5)
            };

            // Create roll dice button
            rollButton = new Button
            {
                Text = "ROLL DICE",
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Arial", 12, FontStyle.Bold),
                BackColor = Color.DarkGreen,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            rollButton.Click += button1_Click;

            // Create die 1 label
            die1Label = new Label
            {
                Text = "?",
                Font = new Font("Arial", 48, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(80, 80),
                Location = new Point(10, 70)
            };

            // Create die 2 label
            die2Label = new Label
            {
                Text = "?",
                Font = new Font("Arial", 48, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Size = new Size(80, 80),
                Location = new Point(100, 70)
            };

            dicePanel.Controls.AddRange(new Control[] { rollButton, die1Label, die2Label });

            rightPanel.Controls.Add(whiteOutPanel, 0, 0);
            rightPanel.Controls.Add(dicePanel, 0, 1);

            mainLayout.Controls.Add(rightPanel, 2, 0);
        }

        // Initializes the game state and board
        private void InitializeGame()
        {
            gameBoard = new Board();
            dice = new Dice();

            player1 = new Player("Player 1", "White");
            player2 = new Player("Player 2", "Black");
            currentPlayer = player1;

            die1Value = 0;
            die2Value = 0;
            diceRolled = false;
            selectedPiece = null;
            selectedTriangle = null;

            UpdateTurnDisplay();
            PlaceInitialPieces();
        }

        // Custom paint handler for drawing triangles
        private void Triangle_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not PictureBox pb || pb.Tag is not Point coords) return;

            int row = coords.X;
            int col = coords.Y;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Alternate colors for triangles
            Color light = Color.FromArgb(190, 140, 80);
            Color dark = Color.FromArgb(90, 40, 35);
            using var brush = new SolidBrush(( col % 2 == 0) ? light : dark);
            using var brush1 = new SolidBrush(( col % 2 == 0) ? dark : light);
            using var pen = new Pen(Color.FromArgb(40, 40, 40), 1.5f);
            
            // Draw triangle pointing down (row 0) or up (row 1)
            Point[] pts = (row == 0)
                ? new[] { new Point(0, 0), new Point(pb.Width, 0), new Point(pb.Width / 2, pb.Height - 100) }
                : new[] { new Point(0, pb.Height), new Point(pb.Width, pb.Height), new Point(pb.Width / 2, 100) };
            if (row == 0)
            {
                g.FillPolygon(brush, pts);
                g.DrawPolygon(pen, pts);
            }
            else
            {
                g.FillPolygon(brush1, pts);
                g.DrawPolygon(pen, pts);
            }
        }
        
        // Custom paint handler for drawing the center bar
        private void CenterBar_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int width = panel.Width;
            int height = panel.Height;
            int centerX = width / 2;
            
            // Draw background
            using (var bgBrush = new SolidBrush(Color.SaddleBrown))
            {
                g.FillRectangle(bgBrush, 0, 0, width, height);
            }
            
            // Draw vertical center line
            using (var pen = new Pen(Color.FromArgb(139, 69, 19), 3))
            {
                g.DrawLine(pen, centerX, 0, centerX, height);
            }
            
            // Draw decorative horizontal lines
            using (var decorPen = new Pen(Color.FromArgb(160, 82, 45), 2))
            {
                int spacing = height / 8;
                for (int i = 1; i < 8; i++)
                {
                    int y = i * spacing;
                    g.DrawLine(decorPen, 5, y, width - 5, y);
                }
            }
            
            // Draw border
            using (var borderPen = new Pen(Color.FromArgb(0, 0, 0), 2))
            {
                g.DrawRectangle(borderPen, 2, 2, width - 4, height - 4);
            }
            
            // Count pieces on bar
            int blackBarCount = gameBoard.pieces.Count(p => p.Position == BLACK_BAR_POSITION);
            int whiteBarCount = gameBoard.pieces.Count(p => p.Position == WHITE_BAR_POSITION);

            int pieceSize = 35;
            int spacing2 = 5;
            
            // Draw black pieces on bar (top)
            for (int i = 0; i < blackBarCount; i++)
            {
                int y = 50 + i * (pieceSize + spacing2);
                int x = (width - pieceSize) / 2;

                g.FillEllipse(Brushes.Black, x, y, pieceSize, pieceSize);
                using (var whitePen = new Pen(Color.White, 3))
                {
                    g.DrawEllipse(whitePen, x, y, pieceSize, pieceSize);
                }
            }
            
            // Draw white pieces on bar (bottom)
            for (int i = 0; i < whiteBarCount; i++)
            {
                int y = height - 50 - (i + 1) * (pieceSize + spacing2);
                int x = (width - pieceSize) / 2;

                g.FillEllipse(Brushes.White, x, y, pieceSize, pieceSize);
                using (var blackPen = new Pen(Color.Black, 3))
                {
                    g.DrawEllipse(blackPen, x, y, pieceSize, pieceSize);
                }
            }
            
            // Draw BAR text
            string text = "BAR";
            using (var font = new Font("Arial", 12, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.FromArgb(210, 180, 140)))
            {
                var textSize = g.MeasureString(text, font);
                float textX = (width - textSize.Width) / 2;
                float textY = (height - textSize.Height) / 2;

                // Draw shadow
                using (var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    g.DrawString(text, font, shadowBrush, textX + 2, textY + 2);
                }

                g.DrawString(text, font, brush, textX, textY);
            }
        }

        // Places all pieces in their initial positions on the board
        private void PlaceInitialPieces()
        {
            // Clear all visual pieces from triangles
            for (int row = 0; row < 2; row++)
                for (int col = 0; col < 12; col++)
                {
                    piecesOnBoard[row, col].Clear();
                    triangles[row, col].Controls.Clear();
                    triangles[row, col].Invalidate();
                }

            // Clear all OUT and BAR piece lists
            whiteOutPieces.Clear();
            blackOutPieces.Clear();
            whiteBarPieces.Clear();
            blackBarPieces.Clear();

            // Remove existing OUT pieces from panels
            var whiteControlsToRemove = whiteOutPanel.Controls.Cast<Control>().Where(c => c != whiteOutLabel).ToList();
            foreach (var ctrl in whiteControlsToRemove) whiteOutPanel.Controls.Remove(ctrl);
            var blackControlsToRemove = blackOutPanel.Controls.Cast<Control>().Where(c => c != blackOutLabel).ToList();
            foreach (var ctrl in blackControlsToRemove) blackOutPanel.Controls.Remove(ctrl);

            // Group pieces by position
            var pieceGroups = gameBoard.pieces.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.ToList());

            // Place each group of pieces
            foreach (var group in pieceGroups)
            {
                int boardPosition = group.Key;

                // Regular board positions (0-23)
                if (boardPosition >= 0 && boardPosition < 24)
                {
                    var (row, col) = BoardToGrid(boardPosition);
                    var list = group.Value;
                    for (int i = 0; i < list.Count; i++)
                        AddPieceToTriangle(row, col, list[i].Color, i, list.Count);
                    RelayoutTriangle(row, col);
                }
                // White OUT position
                else if (boardPosition == 24)
                {
                    for (int i = 0; i < group.Value.Count; i++)
                        AddPieceToOutPanel(whiteOutPanel, "White", whiteOutPieces);
                }
                // Black OUT position
                else if (boardPosition == -1)
                {
                    for (int i = 0; i < group.Value.Count; i++)
                        AddPieceToOutPanel(blackOutPanel, "Black", blackOutPieces);
                }
                // White BAR position
                else if (boardPosition == WHITE_BAR_POSITION)
                {
                    for (int i = 0; i < group.Value.Count; i++)
                        AddPieceToBarPanel(whiteBarPieces, "White");
                }
                // Black BAR position
                else if (boardPosition == BLACK_BAR_POSITION)
                {
                    for (int i = 0; i < group.Value.Count; i++)
                        AddPieceToBarPanel(blackBarPieces, "Black");
                }
            }

            UpdateOutLabels();
        }

        // Adds a visual piece to a triangle on the board
        private void AddPieceToTriangle(int row, int col, string color, int indexInStack, int totalInStack)
        {
            // Skip if triangle is too small
            if (triangles[row, col].Width <= 10) return;

            // Create piece PictureBox
            var piece = new PictureBox
            {
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom,
                Tag = color
            };

            // Add to triangle and setup click handler
            triangles[row, col].Controls.Add(piece);
            piecesOnBoard[row, col].Add(piece);
            piece.Click += Piece_Click;

            RelayoutTriangle(row, col);
        }

        // Creates a bitmap image for a game piece
        private Bitmap MakePieceBitmap(int size, string color)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int d = size - 4;
            
            // Draw black or white piece
            if (color == "Black")
            {
                g.FillEllipse(Brushes.Black, 2, 2, d, d);
                using var pen = new Pen(Color.White, 3);
                g.DrawEllipse(pen, 2, 2, d, d);
            }
            else
            {
                g.FillEllipse(Brushes.White, 2, 2, d, d);
                using var pen = new Pen(Color.Black, 3);
                g.DrawEllipse(pen, 2, 2, d, d);
            }
            return bmp;
        }

        // Repositions and resizes all pieces in a triangle
        private void RelayoutTriangle(int row, int col)
        {
            var cell = triangles[row, col];
            int w = Math.Max(0, cell.Width);
            int h = Math.Max(0, cell.Height);
            var list = piecesOnBoard[row, col];
            
            // Skip if cell is too small or empty
            if (w == 0 || h == 0 || list.Count == 0)
            {
                cell.Invalidate();
                return;
            }

            int margin = 8;
            int pieceSize = Math.Max(10, Math.Min(40, w - 8));
            int tightSpacing = 40;

            // Position each piece in the stack
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                string color = (string)p.Tag;

                p.Width = pieceSize;
                p.Height = pieceSize;
                p.Image = MakePieceBitmap(pieceSize, color);

                // Calculate Y position based on row (top or bottom triangle)
                int y = (row == 0)
                    ? margin + i * tightSpacing
                    : h - margin - pieceSize - i * tightSpacing;

                int x = (w - pieceSize) / 2;
                p.Location = new Point(x, y);
                p.BringToFront();
            }

            cell.Invalidate();
        }

        // Adds a visual piece to an OUT panel
        private void AddPieceToOutPanel(Panel outPanel, string color, List<PictureBox> outList)
        {
            int pieceSize = 45;

            // Create piece PictureBox
            PictureBox piece = new PictureBox
            {
                Width = pieceSize,
                Height = pieceSize,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom,
                Tag = color
            };
            
            // Create piece bitmap
            Bitmap bmp = new Bitmap(pieceSize, pieceSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int margin = 3;
                int diameter = pieceSize - (margin * 2);

                // Draw piece based on color
                if (color == "Black")
                {
                    g.FillEllipse(Brushes.Black, margin, margin, diameter, diameter);
                    using (var whitePen = new Pen(Color.White, 4))
                    {
                        g.DrawEllipse(whitePen, margin, margin, diameter, diameter);
                    }
                }
                else
                {
                    g.FillEllipse(Brushes.White, margin, margin, diameter, diameter);
                    using (var blackPen = new Pen(Color.Black, 4))
                    {
                        g.DrawEllipse(blackPen, margin, margin, diameter, diameter);
                    }
                }
            }
            piece.Image = bmp;
            
            // Position piece in grid layout (3 pieces per row)
            int count = outList.Count;
            int piecesPerRow = 3;
            int spacing = 10;

            int x = 20 + (count % piecesPerRow) * (pieceSize + spacing);
            int y = 90 + (count / piecesPerRow) * (pieceSize + spacing);

            piece.Location = new Point(x, y);

            outList.Add(piece);
            outPanel.Controls.Add(piece);
            piece.BringToFront();
        }

        // Adds a visual piece to the BAR panel (unused in current implementation)
        private void AddPieceToBarPanel(List<PictureBox> barList, string color)
        {
            int pieceSize = 45;

            // Create piece PictureBox
            PictureBox piece = new PictureBox
            {
                Width = pieceSize,
                Height = pieceSize,
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom,
                Tag = color
            };
            
            // Create piece bitmap
            Bitmap bmp = new Bitmap(pieceSize, pieceSize);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                int margin = 3;
                int diameter = pieceSize - (margin * 2);

                // Draw piece based on color
                if (color == "Black")
                {
                    g.FillEllipse(Brushes.Black, margin, margin, diameter, diameter);
                    using (var whitePen = new Pen(Color.White, 4))
                    {
                        g.DrawEllipse(whitePen, margin, margin, diameter, diameter);
                    }
                }
                else
                {
                    g.FillEllipse(Brushes.White, margin, margin, diameter, diameter);
                    using (var blackPen = new Pen(Color.Black, 4))
                    {
                        g.DrawEllipse(blackPen, margin, margin, diameter, diameter);
                    }
                }
            }
            piece.Image = bmp;
            
            // Position piece (layout depends on color)
            int count = barList.Count;
            int x = (color == "White") ? 20 + count * (pieceSize + 5) : 1120 - (count % 3) * (pieceSize + 5);
            int y = (color == "White") ? 90 : 470;

            piece.Location = new Point(x, y);

            barList.Add(piece);
            (color == "White" ? whiteOutPanel : blackOutPanel).Controls.Add(piece);
            piece.BringToFront();
        }

        // Updates the OUT labels with current piece counts
        private void UpdateOutLabels()
        {
            int whiteOutCount = gameBoard.pieces.Count(p => p.Position == WHITE_OUT_POSITION);
            int blackOutCount = gameBoard.pieces.Count(p => p.Position == BLACK_OUT_POSITION);

            whiteOutLabel.Text = $"WHITE OUT\n(Click to bear off)\n({whiteOutCount}/15)";
            blackOutLabel.Text = $"BLACK OUT\n(Click to bear off)\n({blackOutCount}/15)";
        }
        
        // Highlights the selected piece visually
        private void HighlightSelectedPiece(PictureBox piece)
        {
            // Reset all highlights
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 12; col++)
                {
                    foreach (var p in piecesOnBoard[row, col])
                    {
                        p.BackColor = Color.Transparent;
                    }
                }
            }

            // Highlight selected piece
            piece.BackColor = Color.FromArgb(150, Color.Gold);
        }

        #endregion

        #region Click Handlers for Pieces and Triangles
        
        // Handles clicks on game pieces
        private void Piece_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox piece && piece.Tag is string color)
            {
                // Only allow selecting current player's pieces
                if (color == currentPlayer.Color)
                {
                    // Find which triangle contains this piece
                    for (int row = 0; row < 2; row++)
                    {
                        for (int col = 0; col < 12; col++)
                        {
                            if (piecesOnBoard[row, col].Contains(piece))
                            {
                                int boardPosition = GridToBoard(row, col);

                                // Get all pieces at this position
                                var piecesAtPosition = gameBoard.pieces
                                    .Where(p => p.Position == boardPosition && p.Color == color)
                                    .ToList();

                                if (!piecesAtPosition.Any())
                                {
                                    return;
                                }

                                // Only allow selecting the top piece
                                var topPieceVisual = piecesOnBoard[row, col].LastOrDefault();
                                if (topPieceVisual == piece && piecesAtPosition.Any())
                                {
                                    var pieceToSelect = piecesAtPosition.Last();

                                    selectedPiece = pieceToSelect;
                                    selectedTriangle = new Point(row, col);
                                    HighlightSelectedPiece(piece);
                                    UpdateStatusMessage();
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        // Handles clicks on triangle positions
        private void Triangle_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox triangle && triangle.Tag is Point coords)
            {
                int row = coords.X;
                int col = coords.Y;
                int boardPosition = GridToBoard(row, col);

                // If no piece selected, try to select one from this triangle
                if (selectedPiece == null && piecesOnBoard[row, col].Any())
                {
                    var topPiece = piecesOnBoard[row, col].LastOrDefault();
                    if (topPiece != null && topPiece.Tag is string color && color == currentPlayer.Color)
                    {
                        var pieceAtPosition = gameBoard.pieces
                            .Where(p => p.Position == boardPosition && p.Color == color)
                            .LastOrDefault();

                        if (pieceAtPosition != null)
                        {
                            selectedPiece = pieceAtPosition;
                            selectedTriangle = new Point(row, col);
                            HighlightSelectedPiece(topPiece);
                            UpdateStatusMessage();
                        }
                    }
                }
                // If piece already selected, try to move it here
                else if (selectedPiece != null)
                {
                    HandleMove(boardPosition);
                }
            }
        }

        // Handles roll dice button click
        private void button1_Click(object? sender, EventArgs e)
        {
            RollDice();
        }

        // Handles clicks on white OUT panel for bearing off
        private void WhiteOut_Click(object? sender, EventArgs e)
        {
            // Try to bear off if conditions are met
            if (selectedPiece != null && selectedPiece.Color == "White" && gameBoard.AllWhiteHome && diceRolled)
            {
                HandleMove(24);
            }
            // Show error messages if conditions not met
            else if (selectedPiece != null && selectedPiece.Color == "White")
            {
                if (!gameBoard.AllWhiteHome)
                {
                    MessageBox.Show("All white pieces must be in home board (positions 18-23) before bearing off!",
                        "Cannot Bear Off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (!diceRolled)
                {
                    MessageBox.Show("Please roll the dice first!",
                        "Roll Dice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Handles clicks on black OUT panel for bearing off
        private void BlackOut_Click(object? sender, EventArgs e)
        {
            // Try to bear off if conditions are met
            if (selectedPiece != null && selectedPiece.Color == "Black" && gameBoard.AllBlackHome && diceRolled)
            {
                HandleMove(-1);
            }
            // Show error messages if conditions not met
            else if (selectedPiece != null && selectedPiece.Color == "Black")
            {
                if (!gameBoard.AllBlackHome)
                {
                    MessageBox.Show("All black pieces must be in home board (positions 0-5) before bearing off!",
                        "Cannot Bear Off", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (!diceRolled)
                {
                    MessageBox.Show("Please roll the dice first!",
                        "Roll Dice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Handles clicks on center bar to select pieces for re-entry
        private void CenterBar_Click(object? sender, EventArgs e)
        {
            // Check if dice have been rolled
            if (currentPlayer == null || !diceRolled)
            {
                if (!diceRolled)
                {
                    MessageBox.Show("Please roll the dice first!", "Roll Dice",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            // Get pieces on bar for current player
            int barPosition = currentPlayer.Color == "White" ? WHITE_BAR_POSITION : BLACK_BAR_POSITION;
            var barPieces = gameBoard.pieces.Where(p => p.Position == barPosition && p.Color == currentPlayer.Color).ToList();

            // Select piece from bar if any exist
            if (barPieces.Any())
            {
                selectedPiece = barPieces.Last();
                selectedTriangle = null;
                
                // Display valid entry positions
                string validPositions = "";
                if (currentPlayer.Color == "White")
                {
                    if (die1Value > 0) validPositions += $"Position {die1Value - 1} (die: {die1Value}), ";
                    if (die2Value > 0) validPositions += $"Position {die2Value - 1} (die: {die2Value})";
                }
                else
                {
                    if (die1Value > 0) validPositions += $"Position {24 - die1Value} (die: {die1Value}), ";
                    if (die2Value > 0) validPositions += $"Position {24 - die2Value} (die: {die2Value})";
                }

                this.Text = $"Backgammon - {currentPlayer.Name}'s Turn - BAR piece selected! Valid: {validPositions}";
            }
            else
            {
                MessageBox.Show($"No {currentPlayer.Color} pieces on the BAR!", "No Pieces",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Position Conversions

        // Converts grid coordinates to board position (0-23)
        private int GridToBoard(int row, int col)
        {
            if (row == 0)
                return col + 12;
            else
                return 11 - col;
        }

        // Converts board position to grid coordinates
        private (int row, int col) BoardToGrid(int boardPosition)
        {
            if (boardPosition >= 12)
                return (0, boardPosition - 12);
            else
                return (1, 11 - boardPosition);
        }

        #endregion

        #region Logic to Handle Moves

        // Main method to handle piece movement
        private void HandleMove(int targetPosition)
        {
            // Ensure piece is selected and dice rolled
            if (selectedPiece == null || !diceRolled)
            {
                selectedPiece = null;
                selectedTriangle = null;
                return;
            }

            // Check if player has pieces on bar that must be re-entered first
            int playerBarPosition = currentPlayer.Color == "White" ? WHITE_BAR_POSITION : BLACK_BAR_POSITION;
            bool hasBarPieces = gameBoard.pieces.Any(p => p.Color == currentPlayer.Color && p.Position == playerBarPosition);

            if (hasBarPieces && selectedPiece.Position != playerBarPosition)
            {
                MessageBox.Show($"You must re-enter pieces from the BAR first!",
                    "Invalid Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                selectedPiece = null;
                selectedTriangle = null;
                return;
            }

            int currentPosition = selectedPiece.Position;
            bool isValidMove = false;
            int usedDie = 0;

            // Handle bar re-entry
            if (currentPosition == WHITE_BAR_POSITION || currentPosition == BLACK_BAR_POSITION)
            {
                isValidMove = ValidateBarReentry(targetPosition, out usedDie);
            }
            // Handle white piece movement
            else if (selectedPiece.Color == "White")
            {
                int distance1 = targetPosition - currentPosition;

                // Handle bearing off for white
                if (targetPosition == WHITE_OUT_POSITION && gameBoard.AllWhiteHome && currentPosition >= 18)
                {
                    int exactDistance = 24 - currentPosition;

                    // Check each available move
                    foreach (var move in availableMoves)
                    {
                        // Exact distance match
                        if (exactDistance == move)
                        {
                            isValidMove = true;
                            usedDie = move;
                            break;
                        }
                        // Can use higher die if no pieces further out
                        else if (exactDistance < move && !HasPiecesLowerThan(currentPosition, "White"))
                        {
                            isValidMove = true;
                            usedDie = move;
                            break;
                        }
                    }
                }
                // Handle normal white move
                else if (targetPosition < 24 && targetPosition > currentPosition)
                {
                    if (availableMoves.Contains(distance1))
                    {
                        isValidMove = true;
                        usedDie = distance1;
                    }
                }
            }
            // Handle black piece movement
            else
            {
                int distance1 = currentPosition - targetPosition;

                // Handle bearing off for black
                if (targetPosition == BLACK_OUT_POSITION && gameBoard.AllBlackHome && currentPosition <= 5)
                {
                    int exactDistance = currentPosition + 1;

                    // Check each available move
                    foreach (var move in availableMoves)
                    {
                        // Exact distance match
                        if (exactDistance == move)
                        {
                            isValidMove = true;
                            usedDie = move;
                            break;
                        }
                        // Can use higher die if no pieces further out
                        else if (exactDistance < move && !HasPiecesHigherThan(currentPosition, "Black"))
                        {
                            isValidMove = true;
                            usedDie = move;
                            break;
                        }
                    }
                }
                // Handle normal black move
                else if (targetPosition >= 0 && targetPosition < currentPosition)
                {
                    if (availableMoves.Contains(distance1))
                    {
                        isValidMove = true;
                        usedDie = distance1;
                    }
                }
            }

            // Execute valid move
            if (isValidMove)
            {
                // Group pieces by position
                var pieceGroups = gameBoard.pieces
                    .GroupBy(p => p.Position)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Handle capturing opponent piece
                if (targetPosition != WHITE_OUT_POSITION && targetPosition != BLACK_OUT_POSITION && pieceGroups.ContainsKey(targetPosition))
                {
                    var targetPieces = pieceGroups[targetPosition];
                    if (targetPieces[0].Color != selectedPiece.Color)
                    {
                        // Can't move to position with 2+ opponent pieces
                        if (targetPieces.Count >= 2)
                        {
                            selectedPiece = null;
                            selectedTriangle = null;
                            MessageBox.Show("Cannot move to a position with 2 or more opponent pieces!",
                                "Invalid Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        // Capture single opponent piece
                        else
                        {
                            var capturedPiece = targetPieces[0];
                            int capturedOldPosition = capturedPiece.Position;
                            int barPosition = capturedPiece.Color == "White" ? WHITE_BAR_POSITION : BLACK_BAR_POSITION;
                            capturedPiece.MoveTo(barPosition);

                            UpdatePositionVisuals(capturedOldPosition);
                            UpdatePositionVisuals(barPosition);
                        }
                    }
                }

                // Execute the move
                int oldPosition = selectedPiece.Position;
                gameBoard.MovePiece(selectedPiece, targetPosition);

                UpdatePositionVisuals(oldPosition);
                UpdatePositionVisuals(targetPosition);

                // Remove used die from available moves
                if (availableMoves.Contains(usedDie))
                {
                    availableMoves.Remove(usedDie);
                }

                UpdateDiceDisplayFromAvailableMoves();
                CheckWinCondition();

                // Check if turn is over
                if (availableMoves.Count == 0)              
                {
                    SwitchTurns();
                }
                else
                {
                    // Check if any valid moves remain
                    if (!HasAnyValidMove())
                    {
                        MessageBox.Show($"No valid moves available with remaining dice. Turn skipped.",
                            "No Valid Moves", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SwitchTurns();
                    }
                }
            }
            else
            {
                // Show error for invalid move
                MessageBox.Show($"Invalid move! Available dice: {string.Join(", ", availableMoves)}",
                    "Invalid Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            selectedPiece = null;
            selectedTriangle = null;
        }

        // Checks if there are white pieces further from OUT than the given position
        private bool HasPiecesHigherThan(int position, string color)
        {
            if (color == "White")
            {
                return gameBoard.pieces.Any(p => p.Color == "White" && p.Position > position && p.Position < 24);
            }
            return false;
        }

        // Checks if there are black pieces further from OUT than the given position
        private bool HasPiecesLowerThan(int position, string color)
        {
            if (color == "Black")
            {
                return gameBoard.pieces.Any(p => p.Color == "Black" && p.Position < position && p.Position >= 0);
            }
            return false;
        }

        // Updates visual representation of pieces at a specific board position
        private void UpdatePositionVisuals(int boardPosition)
        {
            // Handle white OUT position
            if (boardPosition == WHITE_OUT_POSITION)
            {
                var toRemove = whiteOutPanel.Controls.Cast<Control>().Where(c => c != whiteOutLabel).ToList();
                foreach (var ctrl in toRemove) whiteOutPanel.Controls.Remove(ctrl);
                whiteOutPieces.Clear();

                var whiteOut = gameBoard.pieces.Where(p => p.Position == WHITE_OUT_POSITION).ToList();
                for (int i = 0; i < whiteOut.Count; i++)
                    AddPieceToOutPanel(whiteOutPanel, "White", whiteOutPieces);

                UpdateOutLabels();
                return;
            }

            // Handle black OUT position
            if (boardPosition == BLACK_OUT_POSITION)
            {
                var toRemove = blackOutPanel.Controls.Cast<Control>().Where(c => c != blackOutLabel).ToList();
                foreach (var ctrl in toRemove) blackOutPanel.Controls.Remove(ctrl);
                blackOutPieces.Clear();

                var blackOut = gameBoard.pieces.Where(p => p.Position == BLACK_OUT_POSITION).ToList();
                for (int i = 0; i < blackOut.Count; i++)
                    AddPieceToOutPanel(blackOutPanel, "Black", blackOutPieces);

                UpdateOutLabels();
                return;
            }

            // Handle BAR positions
            if (boardPosition == WHITE_BAR_POSITION || boardPosition == BLACK_BAR_POSITION)
            {
                centerBar.Invalidate();
                return;
            }

            // Handle regular board positions
            if (boardPosition >= 0 && boardPosition < 24)
            {
                var (row, col) = BoardToGrid(boardPosition);
                piecesOnBoard[row, col].Clear();
                triangles[row, col].Controls.Clear();

                var list = gameBoard.pieces.Where(p => p.Position == boardPosition).ToList();
                for (int i = 0; i < list.Count; i++)
                    AddPieceToTriangle(row, col, list[i].Color, i, list.Count);

                RelayoutTriangle(row, col);
            }
        }

        // Checks if a piece is the furthest from OUT in the home board (unused)
        private bool IsHighestPieceInHome(int position, string color)
        {
            if (color == "White")
            {
                return !gameBoard.pieces.Any(p => p.Color == "White" && p.Position > position && p.Position < 24);
            }
            return false;
        }

        // Checks if a piece is the furthest from OUT in the home board (unused)
        private bool IsLowestPieceInHome(int position, string color)
        {
            if (color == "Black")
            {
                return !gameBoard.pieces.Any(p => p.Color == "Black" && p.Position < position && p.Position >= 0);
            }
            return false;
        }

        // Checks if a player has won the game
        private void CheckWinCondition()
        {
            int whiteOutCount = gameBoard.pieces.Count(p => p.Position == 24);
            int blackOutCount = gameBoard.pieces.Count(p => p.Position == -1);

            // White wins
            if (whiteOutCount == 15)
            {
                MessageBox.Show("White wins! All pieces are out!", "Game Over",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                InitializeGame();
            }
            // Black wins
            else if (blackOutCount == 15)
            {
                MessageBox.Show("Black wins! All pieces are out!", "Game Over",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                InitializeGame();
            }
        }

        // Validates if a piece can re-enter from the bar to the target position
        private bool ValidateBarReentry(int targetPosition, out int usedDie)
        {
            usedDie = 0;

            if (selectedPiece == null) return false;

            // White bar re-entry
            if (selectedPiece.Color == "White" && selectedPiece.Position == WHITE_BAR_POSITION)
            {
                // Check each available move
                foreach (var move in availableMoves)
                {
                    int targetFromMove = move - 1;
                    if (targetPosition == targetFromMove && CanMoveToPosition(targetPosition, "White"))
                    {
                        usedDie = move;
                        return true;
                    }
                }
                return false;
            }

            // Black bar re-entry
            if (selectedPiece.Color == "Black" && selectedPiece.Position == BLACK_BAR_POSITION)
            {
                // Check each available move
                foreach (var move in availableMoves)
                {
                    int targetFromMove = 24 - move;
                    if (targetPosition == targetFromMove && CanMoveToPosition(targetPosition, "Black"))
                    {
                        usedDie = move;
                        return true;
                    }
                }
                return false;
            }

            return false;
        }

        // Switches to the other player's turn
        private void SwitchTurns()
        {
            currentPlayer = (currentPlayer == player1) ? player2 : player1;
            diceRolled = false;
            die1Value = 0;
            die2Value = 0;
            availableMoves.Clear();
            selectedPiece = null;
            selectedTriangle = null;
            UpdateTurnDisplay();
            UpdateDiceDisplay();
        }
        
        // Rolls the dice for the current player
        private void RollDice()
        {
            if (!diceRolled && currentPlayer != null)
            {
                // Roll both dice
                die1Value = dice.Roll();
                die2Value = dice.Roll();
                diceRolled = true;
                usedDiceValues.Clear();
                availableMoves.Clear();

                // Handle doubles (same value on both dice)
                if (die1Value == die2Value)
                {
                    // Add 4 moves for doubles
                    for (int i = 0; i < 4; i++)
                    {
                        availableMoves.Add(die1Value);
                    }

                    MessageBox.Show($"DOUBLES! You rolled {die1Value}-{die2Value}! You can move 4 times with {die1Value}!",
                        "Doubles!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Add 2 normal moves
                    availableMoves.Add(die1Value);
                    availableMoves.Add(die2Value);
                }

                UpdateDiceDisplay();

                // Check if any valid moves exist
                if (!HasAnyValidMove())
                {
                    MessageBox.Show($"No valid moves available with dice {die1Value} and {die2Value}. Turn skipped.",
                        "No Valid Moves", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SwitchTurns();
                }
            }
        }
        
        // Checks if a position can be moved to (not blocked by opponent)
        private bool CanMoveToPosition(int targetPosition, string playerColor)
        {
            var piecesAtTarget = gameBoard.pieces
                .Where(p => p.Position == targetPosition)
                .ToList();

            // Position is empty
            if (!piecesAtTarget.Any())
                return true;

            // Position has player's own pieces
            if (piecesAtTarget[0].Color == playerColor)
                return true;

            // Position has single opponent piece (can capture)
            if (piecesAtTarget.Count == 1 && piecesAtTarget[0].Color != playerColor)
                return true;

            // Position is blocked (2+ opponent pieces)
            return false;
        }
        
        // Checks if the current player has any valid moves with remaining dice
        private bool HasAnyValidMove()
        {
            int playerBarPosition = currentPlayer.Color == "White" ? WHITE_BAR_POSITION : BLACK_BAR_POSITION;
            var barPieces = gameBoard.pieces.Where(p => p.Color == currentPlayer.Color && p.Position == playerBarPosition).ToList();


            // If player has pieces on bar, must re-enter first
            if (barPieces.Any())
            {
                foreach (var move in availableMoves)
                {
                    int targetPosition = currentPlayer.Color == "White" ? (move - 1) : (24 - move);
                    if (CanMoveToPosition(targetPosition, currentPlayer.Color))
                        return true;
                }
                return false;
            }

            // Check all player pieces on the board
            var playerPieces = gameBoard.pieces.Where(p =>
                p.Color == currentPlayer.Color &&
                p.Position >= 0 && p.Position < 24
            ).ToList();

            foreach (var piece in playerPieces)
            {
                int currentPosition = piece.Position;
                foreach (var move in availableMoves)
                {
                    if (currentPlayer.Color == "White")
                    {
                        int targetPosition = currentPosition + move;
                        
       
                        if (gameBoard.AllWhiteHome)
                        {
                            int exactDistance = 24 - currentPosition;
           
                            if (move == exactDistance)
                                return true;
                            
    
                            if (move > exactDistance && !HasPiecesHigherThan(currentPosition, "White"))
                                return true;
                        }
                        

                        if (targetPosition < 24 && CanMoveToPosition(targetPosition, "White"))
                        {
                            return true;
                        }
                    }
                    else 
                    {
                        int targetPosition = currentPosition - move;
                        
                       

                        if (gameBoard.AllBlackHome)
                        {
                            int exactDistance = currentPosition + 1;
                            

                            if (move == exactDistance)
                                return true;
                            

                            if (move > exactDistance && !HasPiecesLowerThan(currentPosition, "Black"))
                                return true;
                        }

                        if (targetPosition >= 0 && CanMoveToPosition(targetPosition, "Black"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Display Updates
        private void UpdateTurnDisplay()
        {
            this.Text = $"Backgammon - {currentPlayer.Name}'s Turn ({currentPlayer.Color})";
        }

        private void UpdateStatusMessage()
        {
            if (selectedPiece != null)
            {

                if (selectedPiece.Position == WHITE_BAR_POSITION || selectedPiece.Position == BLACK_BAR_POSITION)
                {
                    string entryInfo = selectedPiece.Color == "White"
                        ? $"Enter at position {(die1Value > 0 ? (die1Value - 1).ToString() : "?")} or {(die2Value > 0 ? (die2Value - 1).ToString() : "?")}"
                        : $"Enter at position {(die1Value > 0 ? (24 - die1Value).ToString() : "?")} or {(die2Value > 0 ? (24 - die2Value).ToString() : "?")}";
                    this.Text = $"Backgammon - {currentPlayer.Name}'s Turn - BAR piece selected - {entryInfo}";
                    return;
                }

                string bearOffMessage = "";
                if (selectedPiece.Color == "White" && gameBoard.AllWhiteHome)
                {
                    bearOffMessage = " - Click WHITE OUT to bear off!";
                }
                else if (selectedPiece.Color == "Black" && gameBoard.AllBlackHome)
                {
                    bearOffMessage = " - Click BLACK OUT to bear off!";
                }

                this.Text = $"Backgammon - {currentPlayer.Name}'s Turn - Piece selected at position {selectedPiece.Position}{bearOffMessage}";
            }
            else
            {
                UpdateTurnDisplay();
            }
        }
        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Backgammon", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void UpdateDiceDisplay()
        {
            if (availableMoves.Count == 0)
            {
                die1Label.Text = "?";
                die2Label.Text = "?";
                die1Label.ForeColor = Color.Gray;
                die2Label.ForeColor = Color.Gray;
                return;
            }

            var groupedMoves = availableMoves.GroupBy(m => m).ToDictionary(g => g.Key, g => g.Count());

            if (groupedMoves.Count == 1 && groupedMoves.First().Value == 4)
            {
                var dieValue = groupedMoves.First().Key;
                die1Label.Text = dieValue.ToString();
                die2Label.Text = dieValue.ToString();
                die1Label.ForeColor = Color.DarkGreen;
                die2Label.ForeColor = Color.DarkGreen;
            }
            else
            {
                die1Label.Text = die1Value.ToString();
                die2Label.Text = die2Value.ToString();
                die1Label.ForeColor = Color.Black;
                die2Label.ForeColor = Color.Black;
            }
        }

        private void UpdateDiceDisplayFromAvailableMoves()
        {
            if (availableMoves.Count == 0)
            {
                die1Label.Text = "✓";
                die2Label.Text = "✓";
                die1Label.ForeColor = Color.Green;
                die2Label.ForeColor = Color.Green;
                return;
            }


            var groupedMoves = availableMoves.GroupBy(m => m).ToDictionary(g => g.Key, g => g.Count());

            if (groupedMoves.Count == 1)
            {
                var dieValue = groupedMoves.First().Key;
                var count = groupedMoves.First().Value;

                die1Label.Text = dieValue.ToString();
                die2Label.Text = $"?";
                die1Label.ForeColor = Color.Black;
                die2Label.ForeColor = Color.DarkBlue;
            }
            else 
            {
                var movesList = availableMoves.ToList();
                die1Label.Text = movesList.Count > 0 ? movesList[0].ToString() : "✓";
                die2Label.Text = movesList.Count > 1 ? movesList[1].ToString() : "✓";

                die1Label.ForeColor = movesList.Count > 0 ? Color.Black : Color.Green;
                die2Label.ForeColor = movesList.Count > 1 ? Color.Black : Color.Green;
            }
        }

        #endregion


    }
}
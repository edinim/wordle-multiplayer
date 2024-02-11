using Blazored.LocalStorage;
using HashidsNet;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Models.Enums;
using wordle_multiplayer.Components.Layout;

namespace Services
{

    public class GameService
    {
        private readonly MemoryCacheService _memoryCache;
        private readonly ILocalStorageService _localStorageService;
        private readonly SolutionWordService _solutionWordService;
        private static int numOfRows = 5;
        private static int numOfCols = 5;

        private string _roomId = "";
        private string _solutionWord = "";
        private List<string> _currentWordArr = [];
        private string _currentWord => string.Join("", _currentWordArr);

        private int _currentRow = 0;
        private int currentColumn => _currentWordArr.Count;
        private HubConnection _hubConnection;
        private Hashids _hashIds;

        public Cell[,] Board = new Cell[numOfRows, numOfCols];

        public Player Player = null;
        public List<Player> Players = [];
        public GameState GameState = GameState.Waiting;
        public string Room => _roomId;
        public string SolutionWord => _solutionWord;
        public int TotalTries = 5;

        public GameService(MemoryCacheService memoryCache, ILocalStorageService localStorageService,
        SolutionWordService solutionWordService)
        {
            _memoryCache = memoryCache;
            _localStorageService = localStorageService;
            _solutionWordService = solutionWordService;
            _hashIds = new Hashids("mysecretsalt", 4, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        public async Task CreateRoom(Player player, HubConnection hubConnection)
        {

            _roomId = _hashIds.Encode(new Random().Next(1000));

            _hubConnection = hubConnection;

            Player = player;

            await SetupBoard();
            await SetupPlayers();


            await AddPlayer(_roomId, player);
        }

        public async Task JoinRoom(string roomId, Player player, HubConnection hubConnection)
        {
            _roomId = roomId;

            _hubConnection = hubConnection;

            Player = player;

            await SetupBoard();
            await SetupPlayers();


            await AddPlayer(_roomId, player);

        }

        public async Task Start()
        {
            GameState = GameState.Playing;
            _solutionWord = await _memoryCache.GetAsync<string>($"room:{_roomId}:solution-word", TimeSpan.FromHours(1), () =>
            {
                return _solutionWordService.GetRandomWord("en");
            });

        }

        public async Task NextRound()
        {
            _memoryCache.Remove($"room:{_roomId}:board:player:{Player.Id}");

            await SetupBoard();
            await Start();

            _currentWordArr = [];
            _currentRow = 0;
            TotalTries = 5;

        }


        public async Task AddPlayer(string roomId, Player player)
        {
            if (GameState == GameState.Playing) return;
            var playerSameId = Players.FirstOrDefault(x => x.Id == player.Id);
            if (playerSameId != null)
                Players.Remove(playerSameId);

            Players.Add(player);

            await _memoryCache.SetAsync<List<Player>>($"room:{roomId}:players", TimeSpan.FromHours(1), Players);

        }

        public void AddLetter(string letter)
        {
            if (GameState != GameState.Playing) return;
            if (_currentWordArr.Count == 5) return;
            if (TotalTries == 0) return;

            Board[_currentRow, currentColumn].Value = letter;
            _currentWordArr.Add(letter.ToLower());
        }

        public void Backspace()
        {
            if (GameState != GameState.Playing) return;
            if (_currentWordArr.Count == 0) return;
            if (TotalTries == 0) return;
            Board[_currentRow, currentColumn - 1].Value = null;
            _currentWordArr.RemoveAt(_currentWordArr.Count - 1);
        }

        public async Task CheckSolution()
        {
            if (GameState != GameState.Playing) return;
            if (_currentWordArr.Count != 5) return;
            if (TotalTries == 0) return;
            if (!_solutionWordService.IsWord("en", _currentWord)) return;

            TotalTries -= 1;

            var cellStates = GetCellStates(_currentWord, _solutionWord);


            if (cellStates.All(x => x == CellState.Correct))
            {
                await RoundFinished(Player.Username);
            }

            for (var i = 0; i < cellStates.Count; i++)
            {
                Board[_currentRow, i].State = cellStates[i];

            }

            _currentRow = _currentRow < 4 ? _currentRow + 1 : _currentRow;

            await _hubConnection.SendAsync("ChatProgress", _roomId, Player.Username, cellStates);

            _currentWordArr = [];


        }

        public async Task RoundTimerEnded()
        {
            await RoundFinished(null);
            _currentWordArr = [];
            _currentRow = 0;

        }

        private async Task RoundFinished(string winner)
        {
            GameState = GameState.RoundFinished;
            await _hubConnection.SendAsync("RoundFinished", _roomId, new Round(1, _solutionWord, winner, 0));
            await NextWord();
        }

        private async Task NextWord()
        {
            string nextSolutionWord = null;
            do
            {
                _solutionWord = _solutionWordService.GetRandomWord("en");
            } while (nextSolutionWord == _solutionWord);
            await _memoryCache.SetAsync($"room:{_roomId}:solution-word", TimeSpan.FromHours(1), _solutionWord);
        }

        private List<CellState> GetCellStates(string word, string solutionWord)
        {
            var cellStates = new List<CellState>();

            var wrongPositionLetters = new List<string>();

            for (var i = 0; i < word.Length; i++)
            {
                if (word[i].ToString().Equals(solutionWord[i].ToString()))
                    cellStates.Add(CellState.Correct);
                else if (solutionWord.Contains(word[i].ToString()) && !wrongPositionLetters.Contains(word[i].ToString()))
                {
                    cellStates.Add(CellState.WrongPosition);
                    wrongPositionLetters.Add(word[i].ToString());
                }
                else
                    cellStates.Add(CellState.Incorrect);

            }

            return cellStates;
        }

        private async Task SetupPlayers()
        {
            Players = await _memoryCache.GetAsync<List<Player>>($"room:{_roomId}:players", TimeSpan.FromHours(1), () => new List<Player>());
        }
        private async Task SetupBoard()
        {

            Func<Cell[,]> DefaultBoard = () =>
            {
                var board = new Cell[numOfRows, numOfCols];
                for (var i = 0; i < board.GetLength(0); i++)
                {
                    for (var j = 0; j < board.GetLength(1); j++)
                    {
                        board[i, j] = new Cell() { Value = null };
                    }
                }

                return board;
            };

            Board = await _memoryCache.GetAsync<Cell[,]>($"room:{_roomId}:board:player:{Player.Id}", TimeSpan.FromHours(1), DefaultBoard);

        }


    }
}
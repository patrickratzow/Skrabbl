﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Skrabbl.API.Services.TurnService;
using Skrabbl.Model;

namespace Skrabbl.API.Test.Services
{
    class TurnServiceSpec
    {
        private int gameId = 5;
        private Game game = new Game();
        private int turnInterval = 500;
        private int letterInterval = 10;
        private string currentWord = "Cake";

        [Test]
        public async Task TimerElapsed_Success()
        {
            //Arrange
            var service = new TurnService(null);
            var timer = service.CreateTurnTimer(gameId, currentWord, turnInterval, letterInterval);
            var cts = new CancellationTokenSource();
            bool hasElapsed = false;

            //Act
            timer.TurnOver += (s, e) =>
            {
                hasElapsed = true;

                cts.Cancel();
            };
            timer.Start();
            try
            {
                await Task.Delay(turnInterval + 250, cts.Token);
            }
            catch
            {
                // Ignored
            }

            timer.Stop();

            //Assert
            Assert.IsTrue(hasElapsed);
        }

        [Test]
        public async Task FoundALetterEvent_Success()
        {
            var service = new TurnService(null);
            var timer = service.CreateTurnTimer(gameId, currentWord, turnInterval, letterInterval);
            var cts = new CancellationTokenSource();
            char foundLetter = 'å';

            //Act
            timer.ShouldSendLetter = () => true;
            timer.FoundLetter += (s, e) =>
            {
                foundLetter = e.Letter;

                cts.Cancel();
            };
            timer.Start();
            try
            {
                await Task.Delay(letterInterval + 250, cts.Token);
            }
            catch
            {
                // Ignored
            }

            timer.Stop();

            // Assert
            Assert.AreNotEqual(foundLetter, 'å');
        }

        [Test]
        public async Task AllLettersHaveBeenIndentifiedAndTurnOverHasBeenCalled()
        {
            //Arrange 
            string longerWord = "fpewpgfowegoprejmgpiwemjpigwmi";
            var service = new TurnService(null);
            var timer = service.CreateTurnTimer(gameId, longerWord, turnInterval, letterInterval);
            bool turnIsOver = false;
            var cts = new CancellationTokenSource();
            char[] foundLetter = new char[longerWord.Length];
            
            //Act
            timer.ShouldSendLetter = () => true; 
            timer.FoundLetter += (o, e) => 
            {
                foundLetter[e.Index] = e.Letter;
            };
            timer.TurnOver += (o, e) =>
            {
                turnIsOver = true;
                cts.Cancel();
            };

            timer.Start();
            try
            {
                await Task.Delay(turnInterval + 250, cts.Token);
            }
            catch
            {
                // Ignored
            }

            timer.Stop();

            //Assert
            //Hack to remove null terminators
            var result = new string(foundLetter).Replace("\0", "") + "";
            Assert.AreEqual(result, longerWord);
            Assert.True(turnIsOver);
        }
    }
}
using System;
using System.Windows.Input;
using System.Windows.Navigation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = System.Windows.Point;

namespace Volleyball
{
    class Game
    {
        private readonly GamePage _page;
        private Point _left, _rigth,_ball;
        private int _leftScore =0 , _rightScore = 0;
        private const double gravity = 500, friction = 0.8;
        private Point _ballVelocity=new Point();
        private double _leftVelocity = 0, _rightVelocity = 0;
        double middle;
        Random rnd = new Random();

        private int aiMiss = 0;


        public Game(GamePage page)
        {
            _page = page;
            middle = page.ActualHeight/2;
            _left = new Point(32, middle/2);
            _rigth = new Point(32, middle+ middle / 2);

            _ball = new Point(132, middle / 2);
            winner = null;
        }

        public void Update(TimeSpan elapsedTime)
        {
            int sector = GetSector();
            MakeAIMove(sector);
            ProcessMoves(elapsedTime);
            ProcessScore(sector);
            PrcoessBallCollision(_left, _leftVelocity,ref leftWasPlayerCollision);
            PrcoessBallCollision(_rigth, _leftVelocity,ref rightWasPlayerCollision);
            ProcessBorderCollisions(sector);
        }

        private void MakeAIMove(int sector)
        {
            /*if(sector!=2)
                return;*/
            if(_ball.Y<=middle)
                return;

            aiMiss += rnd.Next(5) - 2;

            double y = _ball.Y + aiMiss;
            if (y>middle*2)
            {
                y = middle*2;
            }
            ProcessTouchPoint(new Point(_ball.X,y));
        }

        private void ProcessMoves(TimeSpan elapsedTime)
        {
            var t = elapsedTime.TotalMilliseconds/1000.0;
            //if (fly)
                _ballVelocity.Y -= gravity*t;
            double fricFactor = Math.Pow(friction, t);
            _ballVelocity.X *= fricFactor;
            _ballVelocity.Y *= fricFactor;
            _ball.Y += _ballVelocity.X*t;
            _ball.X += _ballVelocity.Y*t;

            _leftVelocity -= gravity*t;
            _rightVelocity -= gravity*t;

            _left.X += _leftVelocity*t;
            _rigth.X += _rightVelocity*t;

            if (_left.X <= 32)
            {
                _left.X = 32;
                _leftVelocity = 0;
            }

            if (_rigth.X <= 32)
            {
                _rigth.X = 32;
                _rightVelocity = 0;
            }
        }
        
        private int GetSector()
        {
            int sector;
            if (_ball.X > _page.ActualWidth/2)
            {
                sector = 0;
            }
            else if (_ball.Y < middle)
            {
                sector = 1;
            }
            else
            {
                sector = 2;
            }
            return sector;
        }

        /*private bool wasLeftCollision;
        private bool wasRightCollision;
        private bool wasHighCollision;*/
        private bool wasNetCollision;
        private bool leftWasPlayerCollision;
        private bool rightWasPlayerCollision;

        private void ProcessBorderCollisions(int sector)
        {
            //обработка столкновения с левой границей
            if (_ball.Y < ballSize/2)
            {
                _ballVelocity.X = -_ballVelocity.X;
            }
            //обработка столкновения с потолком
            if (_ball.X > _page.ActualWidth - ballSize/2)
            {
                _ballVelocity.Y = -_ballVelocity.Y;
            }
            //обработка столкновения с потолком
            if (_ball.Y >= _page.ActualHeight - ballSize/2)
            {
                if (!wasNetCollision)
                {
                    _ballVelocity.X = -_ballVelocity.X;
                }
                wasNetCollision = true;
            }
            else
            {
                wasNetCollision = false;
            }

            if (_ball.Y >= middle - ballSize/2 - netSize/2 && _ball.Y <= middle + ballSize/2 + netSize/2)
            {
                if (_ball.X < _page.ActualHeight/2 + ballSize/2)
                {
                    if (sector == 0)
                    {
                        _ballVelocity.Y = -_ballVelocity.Y;
                    }
                    else
                    {
                        _ballVelocity.X = -_ballVelocity.X;
                    }
                }
            }
        }

        private bool fly;

        private void PrcoessBallCollision(Point player, double verticalVelocity,ref bool wasCollision)
        {
            double distance = Math.Pow(player.X - _ball.X, 2) + Math.Pow(player.Y - _ball.Y, 2);
            const double collisionDistance = (ballSize / 2 + playerSize / 2) * (ballSize / 2 + playerSize / 2);
            if (collisionDistance >= distance)
            {
                fly = true;
                if (!wasCollision)
                {
                    double a = vectorModule(_ballVelocity);
                    double b = vectorModule(new Point(player.Y - _ball.Y, player.X - _ball.X));
                    double c =
                        vectorModule(new Point(player.Y - _ball.Y - _ballVelocity.X,
                                               player.X - _ball.X - _ballVelocity.Y));
                    double angleCosine = (a*a + b*b - c*c)/(2*a*b);
                    double projectionLength = vectorModule(_ballVelocity)*angleCosine;

                    Point pv = new Point(player.Y - _ball.Y, player.X - _ball.X);
                    pv = new Point(pv.X/b, pv.Y/b);
                    pv.X = -2.5*pv.X*projectionLength;
                    pv.Y = -2.5*pv.Y*projectionLength;

                    _ballVelocity.X += pv.X;
                    _ballVelocity.Y += pv.Y;
                }
                wasCollision = true;
            }
            else
            {
                wasCollision = false;
            }
        }

        double vectorModule(Point p)
        {
            return Math.Sqrt(p.X*p.X + p.Y*p.Y);
        }

        public static string winner;
        public static bool AI;

        private void ProcessScore(int sector)
        {
            if (_ball.X <= 0)
            {
                if (sector != 1)
                {
                    _ball = new Point(232, middle/2);
                    _leftScore++;
                    if (_leftScore >= 5)
                    {
                        _page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        winner = "left";
                    }
                }
                else
                {
                    _ball = new Point(232, middle+ middle / 2);
                    _rightScore++;

                    if (_rightScore >= 5)
                    {
                        _page.NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                        winner = "right";
                    }
                }
                _ballVelocity = new Point();
                fly = false;
            }
        }

        const int ballSize = 48;
        const int playerSize = 96;

        public void Draw()
        {
            _page.spriteBatch.Draw(_page._net,new Rectangle(0,(int) (middle-25),(int) (_page.ActualWidth/2),50),Color.White  );


            int halfPlayerSize = playerSize/2;
            _page.spriteBatch.Draw(_page._player,new Rectangle((int) (_left.X-halfPlayerSize),(int) (_left.Y-halfPlayerSize),playerSize,playerSize), Color.White);
            _page.spriteBatch.Draw(_page._player, new Rectangle((int) (_rigth.X - halfPlayerSize), (int) (_rigth.Y - halfPlayerSize), playerSize, playerSize), Color.White);

            
            _page.spriteBatch.Draw(_page._ball, new Rectangle((int)(_ball.X - ballSize/2), (int)(_ball.Y - ballSize / 2), ballSize, ballSize), Color.White);

            for (int i = 0; i < _leftScore; i++)
            {
                _page.spriteBatch.Draw(_page._ball,new Rectangle((int) (_page.ActualWidth-15),i*15,10,10),Color.White );
            }

            for (int i = 0; i < _rightScore; i++)
            {
                _page.spriteBatch.Draw(_page._ball, new Rectangle((int) (_page.ActualWidth- 15), (int) (_page.ActualHeight- i * 15), 10, 10), Color.White);
            }
            //_page.spriteBatch.Draw(_page._net, new Rectangle(0, 0, 100, 100), Color.White);
        }

        public void TouchFrame(object sender, TouchFrameEventArgs e)
        {
            foreach (var p in e.GetTouchPoints(_page))
            {
                if (p.Position.Y < middle || !AI)
                    ProcessTouchPoint(p.Position);
            }
        }

        const int netSize = 50;
        const int jumpVelocity = 400;

        private void ProcessTouchPoint(Point position)
        {
            
            if (position.Y < middle-netSize)
            {
                _left.Y = position.Y;
                if (position.X > 64 && _left.X <= 32)
                {
                    _leftVelocity =jumpVelocity;
                }
            }

            if (position.Y > middle + netSize)
            {
                _rigth.Y = position.Y;
                if (position.X > 64 && _rigth.X <= 32)
                {
                    _rightVelocity = jumpVelocity;
                }
            }
        }
    }
}
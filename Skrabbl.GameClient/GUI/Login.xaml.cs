﻿using Newtonsoft.Json;
using RestSharp;
using Skrabbl.Model;
using Skrabbl.Model.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Skrabbl.GameClient.GUI
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            if (Properties.Settings.Default.JWT != String.Empty)
            {
                //check if saved JWT is valid
                if (Properties.Settings.Default.JWTExpire < DateTime.Now)
                {
                    if(Properties.Settings.Default.RefreshExpiresAt < DateTime.Now)
                    {
                        //To old refresh and JWT
                    }
                    else
                    {
                        //Valid refresh token but old JWT, generate a new one

                        //7+ dage skal log ind igen
                        //Hver gang programmet startes gives der en ny refresh token
                            //Gør det gennem login post

                    }
                }
                else
                {
                    OpenGameWindow();
                }
            }
        }

        public void UsernameTxtFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text == "Username")
            {
                txtUsername.Text = "";
                txtUsername.Opacity = 1;
            }
        }
        public void PasswordTxtFocus(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Text == "Password")
            {
                txtPassword.Text = "";
                txtPassword.Opacity = 1;
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            //Setup
            txtError.Text = "";
            IRestResponse response_POST;
            RestClient rest_client = new RestClient();
            //5001;
            //NOTE: If the client is not working properly, it either requires ServiceURI to be without or with https. 
            string PortOfTheDay = "50916"; //This port number changes!
            string ServiceURI = "http://localhost:" + PortOfTheDay + "/api/user/login";


            rest_client.BaseUrl = new Uri(ServiceURI);
            RestRequest request_POST = new RestRequest(ServiceURI, Method.POST);
            LoginDto loginData = new LoginDto { Username = txtUsername.Text, Password = txtPassword.Text };

            request_POST.AddJsonBody(loginData);

            response_POST = rest_client.Execute(request_POST);


            HttpStatusCode statusCode = response_POST.StatusCode;
            int integerStatus = (int)statusCode;

            if (integerStatus == 200)
            {
                if (checkBoxRememberMe.IsChecked.Value)
                {
                    LoginResponseDto tokens = JsonConvert.DeserializeObject<LoginResponseDto>(response_POST.Content);
                    SaveTokens(tokens);
                }
                else
                    RemoveTokenValues();

                OpenGameWindow();
            }
            else if (integerStatus == 401)
            {
                txtError.Text = "Wrong combination of username and password";
            }

        }

        private void SaveTokens(LoginResponseDto tokens)
        {
            Properties.Settings.Default.JWT = tokens.Jwt.Token;
            Properties.Settings.Default.JWTExpire = tokens.Jwt.ExpiresAt;

            Properties.Settings.Default.RefreshToken = tokens.RefreshToken.Token;
            Properties.Settings.Default.JWTExpire = tokens.RefreshToken.ExpiresAt;

            Properties.Settings.Default.UserId = tokens.UserId;

            Properties.Settings.Default.Save();
        }

        public void RemoveTokenValues()
        {
            Properties.Settings.Default.JWT = String.Empty;
            Properties.Settings.Default.JWTExpire = DateTime.Now;

            Properties.Settings.Default.RefreshToken = String.Empty;
            Properties.Settings.Default.JWTExpire = DateTime.Now;
            Properties.Settings.Default.Save();
        }

        private LoginResponseDto BuildLoginResponseFromSettings()
        {
            //Building the Token structure
            LoginResponseDto resp = new LoginResponseDto()
            {
                Jwt = new Jwt()
                {
                    Token = Properties.Settings.Default.JWT,
                    ExpiresAt = Properties.Settings.Default.JWTExpire
                },
                RefreshToken = new RefreshToken()
                {
                    Token = Properties.Settings.Default.RefreshToken,
                    ExpiresAt = Properties.Settings.Default.RefreshExpiresAt
                    //Dont know if i should get the user here
                },
                UserId = Properties.Settings.Default.UserId
            };

            return resp;
        }

        private void OpenGameWindow()
        {
            LoginResponseDto resp = BuildLoginResponseFromSettings();
            MainWindow gameWindow = new MainWindow(resp, this);
            gameWindow.Show();
            this.Visibility = Visibility.Hidden;
        }
    }
}

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LeavePrint.aspx.cs" Inherits="qingjia_YiBan.HomePage.imagetest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <meta http-equiv="X-UA-Compatible" content="ie=edge" />

    <link rel="stylesheet" href="css/weui.css" />
    <link rel="stylesheet" href="css/example.css" />
    <link rel="stylesheet" href="css/index.css" />
    <style>
        .app {
            position: absolute;
            top: 0;
            bottom: 0;
            right: 0;
            left: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }

        @media screen and (max-width:600px) {
            .app img {
                width: 50vh;
            }
        }
    </style>
    <title>请假条</title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="top" class="index_top" style="background:white;">
        <a href="../qingjia_WeChat.aspx"><img src="images/icon_back_.png" style="height:40%; margin-left:2%; margin-top:4%;" /></a>
        <a href="../qingjia_WeChat.aspx"><img src="images/icon_home.png" style="height:40%; margin-left:2%; margin-top:4%;"/></a>        
    </div>
    <div class="app">
        <img id="imgLeave" runat="server" src="http://oyzg731sy.bkt.clouddn.com/FlL70dFa87VxKgNSYDJ3AQcfCUr_" alt="请假条" />
    </div>
    </form>
</body>
</html>

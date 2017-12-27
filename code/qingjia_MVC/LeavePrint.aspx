<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LeavePrint.aspx.cs" Inherits="qingjia_MVC.LeavePrint" %>

<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="ie=edge">
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
    <script src="res/js/jquery-1.4.4.min.js"></script>
    <script src="res/js/jquery.jqprint-0.3.js"></script>
    <script type="text/javascript">
        function jprint() {
            $("#leavePic").jqprint();
        }
    </script>
    <title>请假条打印</title>
</head>

<body>
    <div class="app">
        <img runat="server" id="picUrl" src="123" alt="请假条" />
    </div>
</body>
</html>

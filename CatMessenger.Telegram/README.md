# 前置条件
1. TelegramBot
2. Telegram群组
3. 网络代理（可选）
4. RabbitMQ

## TelegramBot

需要与 `@botfather` 对话以创建bot，存储 oken to access the HTTP API 以填充配置文件。

## Telegram群组

需要将机器人加入群组并授予管理员权限以保证可以访问到所有消息。

## 网络代理

需要结合 clashforwindows/v2raya 等使用

## RabbitMQ

与之前的教程相同，需要使用同一个Rabbit配置和服务器。

# 持久运行
通过`systemd`
```service
[Unit]
Description=CatMessenger Telegram Service
After=network.target

[Service]
WorkingDirectory=/home/[]/rabbitMQ/publish
ExecStart=/home/[]/rabbitMQ/publish/CatMessenger.Telegram

# 重启策略配置
Restart=always
# 服务崩溃后的重启延迟时间
RestartSec=5

[Install]
WantedBy=multi-user.target
```
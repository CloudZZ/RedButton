# RedButton

инструкция по redbutton
сервисная учетка права

AD
User OU - Descended User Objects  - Read All objects
User OU - Descended User Objects  - Write "userAccountControl"

Computer
RD Connection Broker - Local Admin
RD Session Host - Local Admin

Web Host with Red Button
Local Admin

IIS
- отдельный app pool с запускаемый от сервисной учетки
- Сконвертировать папку с red button в Application
- поправить конфигурацю Web.config
- ASP.net должен быть установлен

Заметка
- умеет работать только с ou второго уровня

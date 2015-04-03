Ural Federal University
=======================
Computer Graphics and Geometry (Компьютерная Графика и Геометрия)
--------------------------------------------------------------
### *Лахтин А С* ###

Домашние работы по КГГ


inspired by [Roman Dubinin](https://github.com/RomanDubinin/CGG)

**2014-2015 год** (3 курс)


Самые важные результаты изучения ассемблера в папке MyLibs:
  * CmdArg.inc - работа с аргументами командной строки
  * SexyPrnt.inc - набор процедур для удобного вывода
  * ChVideo.inc - смена видео-режима и отображаемой страницы

Остальное — домашние работы и наработки по спецкурсу "Ассемблер" (1 семестр) и "АСВТ" (2 семестр)

> Многие исходники в кодировке Cyrillic Windows 866 (CP 866)
> 
> Имеется скрипт KEYRUS.COM — русификация DOS

Рекомендую:

1. Скачать ftp://cs.usu.edu.ru/DOS.zip — сборник полезных утилит, ассемблеров,
    составленный в УрГУ им. Горького — и разархивировать папку DOS в корень репозитария
    (уже есть в .gitignore)

2. Использовать [DosBox 0.74](http://www.dosbox.com/download.php?main=1):

      * Дописать в конец файла `C:\Users\ %username% \AppData\Local\DOSBox\dosbox-0.74.conf` то,
        что лежит в конце одноименного файла в корне репозитария, примерно:
          * ```
            # Смонтировать С: в корень репозитария,
            # Активация полезных утилит из INCLUDE, BP\BIN, NG
            # Например, go -> .exe, gocom -> .com, td, tdmem
            
            mount C C:\Users\ %username% \Docume~1\GitHub\urfu-asm\
            C:
            set path=%path%;C:\DOS\BP\BIN\;C:\DOS\NG\;C:\INCLUDE\
            NG.EXE
            KEYRUS.COM

            cd 2Semes~1
            cd 4TASK
            gocom 4task 3 0
            ```
      * Не забудь сменить путь до репозитария!


# SpaceShutan

задание было написать десктопную игру на UNet:

Необходимо написать на Unity3D приложение-игру, аналог всем известных космических скроллеров, вида:
https://www.youtube.com/watch?v=MuhOZWWjiAM 
https://www.youtube.com/watch?v=1H2k4891H7w 

Детали реализации:
1 Игра может быть выполнена в 2D или 3D, на ваше усмотрение.
2 На первом экране три кнопки: Старт, Присоединиться и Выйти из игры.  При нажатии на Старт игрока перебрасывает на экран игры. При нажатии на кнопку Выйти из игры приложение должно завершиться. При нажатии на кнопку Присоединиться запускается режим совместной игры (см. Пункт 8 ниже) .
3 На экране игры сверху вниз падают шарики, внизу корабль игрока двигается кнопками влево-вправо. В правом верхнем углу счетчик очков. При нажатии клавиши пробел корабль игрока выпускает снаряд летящий вверх. Если снаряд сталкивается с шариком, игрок получает +1 очко и шарик исчезает. Если корабль игрока сталкивается с шариком, то игра закончена и игрок возвращается в стартовое меню.
4 Шарики должны не просто падать вниз, а лететь по какой-то более сложной траектории.
5 Каждые 15 сек скорость полета шариков и скорость их появления должна увеличиваться.
6 Где то на экране игры разместить кнопку Пауза, при нажатии на которую игра останавливается, а при повторном нажатии - продолжается
7 Добавить какой-нибудь визуальный эффект при попадании снаряда в шарик. 
8 При нажатии на Присоединиться на экране должно открыться окно со строкой ввода. Если ввести в нее IP адрес, то игра должна подключиться к другой игре, запущенной на указанном IP адресе. При этом должен появится еще один корабль игрока, и игра продолжается совместно.
9 Вся визуальная часть (текстуры, эффекты, системы частиц и прочее) - на ваше усмотрение. Но постарайтесь оформить игру симпатично, в Интернете и ассетах Unity есть множество бесплатных текстур и анимаций. Любой дополнительный геймплей, который вы добавите в игру (например другие вражеские корабли, бонусы, и т.д.) будут плюсом.

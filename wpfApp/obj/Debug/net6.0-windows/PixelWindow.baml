   M S B A M L   `   `   `  ����A  =wpfApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:wpfAppwpfApp  5   	       wpfApp.PixelWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:wpfApp     5   	     � Title$%  Изменить пиксель��5   	   $	��250��6"   $	��300��6/    � 
ResizeMode$ CanMinimize=�6;   .��5	      � $��10q�6   .+�5
      �� .+�5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� $	��120��6   $��5q�6   .+�5      �� $)��#Координаты пикселя��6   $��0,0,0,21q�6+   $��Center=�6=    �� TextAlignment$ Center=�6Z   $
��Wrap=�6q   �� 5      $��X��6   $��0,5,0,1q�6   �� 5      -   5         ��Name$ xText��5      �� 5      $��Y��6   $��0,5,0,1q�6   �� 5      -   5        $ yText��5      5      �� 5      $	��120��6   $��5q�6   .+�5      �� $��Цвет пикселя��6   $ Center=�6%   $
��Wrap=�6<   �� 5      $��0,5,0,1q�6   $'��!Величина красного��6"   $ Center=�6;   $
��Wrap=�6R   �� 5      -   5        $ redText��5      �� 5      $��0,5,0,1q�6   $'��!Величина зелёного��6"   $ Center=�6;   $
��Wrap=�6R   �� 5      -   5        $ 	greenText��5      �� 5      $��0,5,0,1q�6   $#��Величина синего��6"   $ Center=�69   $
��Wrap=�6P   �� 5      -   5        $ blueText��5      5      5      �� 5      -   5         ��Name$ ValidationErrorText��5      
���  ��$��0,5,0,0q�6=   $
��Wrap=�6N   5      �� 5!      $ 
Horizontal=�6   $��Right=�6(   $��Bottom=�6D   .+�5"      �� -   5        $
��ОК��5"      $��75��6   $��5q�6$   6/   �� 5#      -   5        $��Отмена��5#      $��75��6   $��5q�6(    �� IsCancel$
 True��6N   5$      5%      5&      
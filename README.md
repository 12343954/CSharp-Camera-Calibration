# Code in Funny! Camera calibration in C# vs Python, Word by word comparison, Explain the parameters in detail

<br />
<br />

<a href='https://www.youtube.com/watch?v=ZZ5M7Q5ZWX4&feature=youtu.be&ab_channel=CoolooAI' target="_blank">
<img alt="" src="./image1.jpg" width="800" />
</a>

https://www.youtube.com/watch?v=ZZ5M7Q5ZWX4&feature=youtu.be&ab_channel=CoolooAI


<a href='https://www.youtube.com/watch?v=ZZ5M7Q5ZWX4&feature=youtu.be&ab_channel=CoolooAI' target="_blank">
<img alt="" src="./image2.jpg" width="800" />
</a>


<br />
<br />

| original | calibrated |
|:-----:|:-----:|
|<img alt="" src="./python/original/image_07_12_19_0000.jpg" width="800" />|<img alt="" src="./python/calibrated/calibrate_142041807408.jpg" width="800" /> |
|<img alt="" src="./python/original/image_07_12_19_3641.jpg" width="800" />|<img alt="" src="./python/calibrated/calibrate_142042174721.jpg" width="800" /> |
|<img alt="" src="./python/original/image_07_12_19_3865.jpg" width="800" />|<img alt="" src="./python/calibrated/calibrate_142042392747.jpg" width="800" /> |
|<img alt="" src="./python/original/image_07_12_19_8340.jpg" width="800" />|<img alt="" src="./python/calibrated/calibrate_142042524115.jpg" width="800" /> |
|<img alt="" src="./python/original/image_07_12_19_8605.jpg" width="800" />|<img alt="" src="./python/calibrated/calibrate_142042661747.jpg" width="800" /> |
|<img alt="" src="./python/original/image_07_12_19_9485.jpg" width="800" />|<img alt="" src="./python/calibrated/calibrate_142042799379.jpg" width="800" /> |

<br>
<hr>
<br>

## 1, download all the files
## 2, run the c# code directly via vs2022
## 3. run the python code after install all the dependences.


<br>
<hr>
<br>

## PS: 

    1. c# code use the latest opencv 4.6.0, little different from the 4.4.5.
    2. python code use two calibration method, 1: udistort()  2:remap(), you can compare the difference between the two.
    3. change the original "Calibration Target" images from you cameras and run it again.


# enjoy it
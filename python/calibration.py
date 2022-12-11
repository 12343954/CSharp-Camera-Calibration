# -*- coding:utf-8 -*-
# region import libs
import cv2
import math
import os
from datetime import datetime
import numpy as np
from sys import platform
import glob
from PIL import Image
import matplotlib.pyplot as plt
# endregion


def isJupyter():
    '''
    In jupyter notebook or in CMD
    '''
    try:
        shell = get_ipython().__class__.__name__
        if shell == 'ZMQInteractiveShell':
            return True  # Jupyter notebook or qtconsole
        elif shell == 'TerminalInteractiveShell':
            return False  # Terminal running IPython
        else:
            return False  # Other type (?)
    except NameError:
        return False  # Probably standard Python interpreter


def display1(img, name, isGray=True, size=(6, 8)):
    '''
    display one image in jupyter
    '''
    plt.figure(figsize=size)  # (w,h)
    if isGray == True:
        plt.imshow(img, cmap='gray')
    else:
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        plt.imshow(img)

    #     plt.xticks([]), plt.yticks([]) # hidden x, y
    plt.title(name)
    plt.show()


def display2(images, cols=4, width=800, height=600):
    '''
    display multi images in jupyter
    '''
    length = len(images)
    rows = math.ceil(length / cols)

    fig = plt.figure(figsize=(32, 32))  # px per inch

    for i in range(1, length + 1):

        #   img = np.random.randint(10, size=(height,width))
        fig.add_subplot(rows, cols, i)
        plt.imshow(images[i - 1])
    plt.show()


In_CMD = not isJupyter()

# Set the parameters for finding sub-pixel corner points,
# and the stop criterion
# adopted is the maximum number of cycles of 30
# and the maximum error tolerance of 0.001
criteria = (cv2.TERM_CRITERIA_MAX_ITER | cv2.TERM_CRITERIA_EPS, 30, 0.001)

# number of inner corner points,
# not the number of black and white boxes
ROW = 9
COL = 6

WIN32 = platform == 'win32'

# create [54,3] matrix
objp = np.zeros((ROW * COL, 3), np.float32)
# create world axis on calibration board, all Z axis = 0, X=0,y=0
objp[:, :2] = np.mgrid[0:ROW, 0:COL].T.reshape(-1, 2)


def calibrate():

    dir_path = os.path.dirname(os.path.realpath(__file__))

    images = glob.glob('./original/image_*.jpg')

    # print(images)
    if len(images) == 0:
        print('Image NOT Found!')
        pass

    obj_points = []  # store 3D points
    img_points = []  # store 2D points
    calibrated_images = []

    for fname in images:
        img = cv2.imread(fname)
        gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        size = gray.shape[::-1]
        ret, corners = cv2.findChessboardCorners(gray, (ROW, COL), None)

        # if found
        if ret:
            obj_points.append(objp)

            # keep finding sub pix
            corners2 = cv2.cornerSubPix(gray, corners, (6, 6), (-1, -1),
                                        criteria)
            if corners2.all:
                img_points.append(corners2)
            else:
                img_points.append(corners)

            cv2.drawChessboardCorners(img, (ROW, COL), corners, ret)

            save_path = os.path.join(dir_path, "calibrated", "calibrate_%s.jpg" % datetime.now().strftime("%H%M%S%f"))
            cv2.imwrite(save_path, img)

            if In_CMD:
                if WIN32:
                    os.system("start %s" % save_path)
                else:
                    os.system("open %s" % save_path)
            else:  # in jupyter
                calibrated_images.append(img)

    if not In_CMD:  # in jupyter
        display2(calibrated_images, cols=3)

    print("%d images found to be processed ..." % len(img_points))

    global mtx, dist, rvecs, tvecs

    # calibrate camera
    ret, mtx, dist, rvecs, tvecs = cv2.calibrateCamera(obj_points, img_points,
                                                       size, None, None)

    print("\nret:", ret)
    print("\ncamera matrix:\n", mtx)  # inner params
    # distortion cofficients = (k_1,k_2,p_1,p_2,k_3)
    print("\ndistortion coefficients:\n", dist)
    # print("\n rotation vectors: \n", rvecs)  # outer params
    # print("\n translation vectors: \n", tvecs)    # outer params

    calculate_error(obj_points, img_points)


def calculate_error(obj_points, img_points):
    # Calculate back projection error
    print("---------Calculate back projection error-----------")
    tot_error = 0
    for i in range(len(obj_points)):
        img_points2, _ = cv2.projectPoints(obj_points[i], rvecs[i], tvecs[i],
                                           mtx, dist)

        error = cv2.norm(img_points[i], img_points2,
                         cv2.NORM_L2) / len(img_points2)
        tot_error += error

    mean_error = tot_error / len(obj_points)
    print("total error: ", tot_error)
    print("mean  error: ", mean_error)


def calibrated_images_undistort(image_path):
    # Distortion correction
    print("Distortion correction --------------------------")
    img = cv2.imread(image_path)

    # Method 1: undistort
    print("\n------------------# Method 1: undistort-------------------")
    h, w = img.shape[:2]
    newcameramtx, roi = cv2.getOptimalNewCameraMatrix(mtx, dist, (w, h), 1,
                                                      (w, h))

    print('newcameramtx', newcameramtx)

    # calibrate image
    dst = cv2.undistort(img, mtx, dist, None, newcameramtx)
    x, y, w, h = roi  # rectangle with calibrated image
    dst1 = dst[y:y + h, x:x + w]  # crop to a new image

    # save_path = os.path.join(dir_path, "calibration", "result",
    #                          "calibresult_f1_%s.jpg" %
    #                          datetime.now().strftime("%H%M%S%f"))

    # cv2.imwrite(save_path, dst1)
    print("Method 1: dst size:", dst1.shape)

    if not In_CMD:
        # print(type(dst1),type(dst1)== np.ndarray )
        # print(type([dst1]),type([dst1]) == list)
        display2([dst, dst1], 2)

    return dst1


def calibrated_images_remap(image):
    # Method 2: Remap
    img = cv2.imread(image)

    # Method 1: undistort
    print("\n------------------# Method 1: undistort-------------------")
    h, w = img.shape[:2]
    newcameramtx, roi = cv2.getOptimalNewCameraMatrix(mtx, dist, (w, h), 1,
                                                      (w, h))

    print('newcameramtx', newcameramtx)

    print("-------------------# Method 2: Remap-----------------------")
    mapx, mapy = cv2.initUndistortRectifyMap(mtx, dist, None, newcameramtx,
                                             (w, h), 5)

    # after remap£¬size reduce
    dst = cv2.remap(img, mapx, mapy, cv2.INTER_CUBIC)
    x, y, w, h = roi
    dst2 = dst[y:y + h, x:x + w]

    # if dst2.size > 0:
    #     save_path = os.path.join(dir_path, "calibration", "result",
    #                              "calibresult_f2_%s.jpg" %
    #                              datetime.now().strftime("%H%M%S%f"))
    #     cv2.imwrite(save_path, dst2)

    print("Method 2: dst size:", dst2.shape)  # smaller than Method 1

    if not In_CMD:
        display2([dst, dst2], 2)

    return dst2


def main():
    calibrate()
    print('\n\ncalibration finished!\n\tselect a calibration method:\n')
    print('\t\t1:  udistort()\n\t\t2:  remap()\n\n')

    while (True):
        input_str = input('\n input a action name:')

        if input_str == 'q':
            break

        if input_str == '1':
            image_path = input(
                'input a image path to calibration(undistort): \n')
            img = calibrated_images_undistort(image_path)
            while True:
                # cv2.namedWindow(
                #     'image', flags=cv2.WINDOW_NORMAL)
                cv2.imshow('image', cv2.imread(image_path))

                # cv2.namedWindow(
                #     'calibrated', flags=cv2.WINDOW_NORMAL)
                cv2.imshow('calibrated', img)
                key = cv2.waitKey(10)
                if key == ord('x'):
                    cv2.destroyAllWindows()
                    break

        elif input_str == '2':
            image_path = input('input a image path to calibration(remap):\n')
            img = calibrated_images_remap(image_path)
            while True:
                # cv2.namedWindow(
                #     'image', flags=cv2.WINDOW_NORMAL)
                cv2.imshow('image', cv2.imread(image_path))

                # cv2.namedWindow(
                #     'calibrated', flags=cv2.WINDOW_NORMAL)
                cv2.imshow('calibrated', img)

                key = cv2.waitKey(10)
                if key == ord('x'):
                    cv2.destroyAllWindows()
                    break


if __name__ == "__main__":
    main()

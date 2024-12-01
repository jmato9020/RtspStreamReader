import os
import cv2
from ultralytics import YOLO
import sys
from PIL import Image
import io
import torch
width = 2048
height = 1080
bitsPerPixel = 32
_imageBitSize = width*height *bitsPerPixel
_imageByteSize = int(_imageBitSize / 8)
os.environ['YOLO_VERBOSE'] = 'False'
model = YOLO("yolov8n.pt",verbose=False)
i = 0
while True:
    image_data = sys.stdin.buffer.read(_imageByteSize)
    image = Image.frombytes('RGBA', (2048,1080), image_data)
    results = model.predict(source=image, verbose=False)  # save plotted images
    #results[0].save('result{0}.png'.format(i))
    results[0].names
    namelist = []
    [[namelist.append(results[0].names[j.item()]) for j in i.cls] for i in results[0].boxes]
    print(''.join(f'{i},' for i in namelist))
    i=i+1
    sys.stdout.flush()
    
    

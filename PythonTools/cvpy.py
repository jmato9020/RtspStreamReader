import cv2
from ultralytics import YOLO
import sys
from PIL import Image
import io
print("Hello", file=sys.stdout)
width = 2048
height = 1080
bitsPerPixel = 32
_imageBitSize = width*height *bitsPerPixel
_imageByteSize = int(_imageBitSize / 8)

model = YOLO("yolov8n.pt")
i = 0
while True:
    print(i)
    image_data = sys.stdin.buffer.read(_imageByteSize)
    print(len(image_data))
    image = Image.frombytes('RGBA', (2048,1080), image_data)
    results = model.predict(source=image, save=True)  # save plotted images
    results[0].save('result{0}.png'.format(i))
    i=i+1
    
    

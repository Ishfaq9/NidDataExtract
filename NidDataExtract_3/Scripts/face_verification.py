import sys
import json
from deepface import DeepFace

def verify_faces(img1_path, img2_path):

    backends = [
        'opencv',
        'ssd',
        'dlib',
        'mtcnn',
        'fastmtcnn',
        'retinaface',
        'mediapipe',
        'yolov8',
        'yunet',
        'centerface',
    ]

    obj = DeepFace.verify(
        img1_path=img1_path,
        img2_path=img2_path,
        detector_backend=backends[0],
        align=True,
    )
    json_result = json.dumps(obj, indent=4)
    print(json_result)

if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Please provide paths for both images.")
    else:
        img1 = sys.argv[1]
        img2 = sys.argv[2]
        verify_faces(img1, img2)

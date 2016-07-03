from flask import Flask, request, jsonify
from sklearn.externals import joblib
import numpy as np
import json
import os
import sys

dir = os.path.dirname(__file__)
app = Flask(__name__)
name=""
if len(sys.argv) >1:
    name=sys.argv[1]

if name == 'knn':
    print ("KNN")
    emotion_recognition = joblib.load(os.path.join(dir,'KNN_emotionrecognition.pkl'))
else:
    print ("SVM")
    emotion_recognition = joblib.load(os.path.join(dir,'SVC_emotionrecognition.pkl'))

@app.route('/predict')
def predict():
    prediction = emotion_recognition.predict( np.fromstring(request.args.get('data', ''), sep=',').reshape(1,-1))[0]
    return json.dumps([prediction])
@app.route('/predict/block')
def predictblock():
    prediction= []
    for x in range(1,21):
        prediction.append( (emotion_recognition.predict( np.fromstring(request.args.get('data'+str(x), ''), sep=',').reshape(1,-1))[0]))
    return json.dumps(prediction)
@app.route('/predict/proba')
def predictproba():
    try:
        prediction = emotion_recognition.predict_proba( np.fromstring(request.args.get('data', ''), sep=',').reshape(1,-1) )[0]
        arr =list(map(str,prediction))
        return json.dumps( [{'angry':arr[0],'disgust':arr[1],'fear':arr[2],'happy':arr[3],'sad':arr[4],'surprise':arr[5] }])
    except AttributeError:
        raise ValueError('No function predict_proba for this classifier')
@app.route('/predict/proba/block')
def predictprobablock():
    try:
        prediction= []
        for x in range(1,21):
            p = emotion_recognition.predict_proba(np.fromstring(request.args.get('data'+str(x), ''), sep=',').reshape(1,-1) )[0]
            arr =list(map(str,p))
            prediction.append( {'angry':arr[0],'disgust':arr[1],'fear':arr[2],'happy':arr[3],'sad':arr[4],'surprise':arr[5] })
        return json.dumps(list(prediction))
    except AttributeError:
        raise ValueError('No function predict_proba for this classifier')

if __name__ == '__main__':
    app.run()
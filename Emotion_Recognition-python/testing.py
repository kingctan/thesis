import sys
from sklearn.externals import joblib
classifier = joblib.load('C:\\Users\Miguel\OneDrive\Documenten\Thesis\Emotion_Recognition-python\KNN_emotionrecognition.pkl')

#argv = sys.argv[1:]
argv =["0.0317999","-0.112755","0.0540595","0.221953","0.00456313","0.0928099","0.000634832","0.400464","0.0443363","0.0773099","0","0.230036","0.0359804","0.0196022","0.0209054","0.027408","0.0928099"]
#print(argv)
#print(dataset[1:])
prediction= []
   # arr = list(map(str,classifier.predict([argv])).reshape(1,-1))[0]
print(classifier.predict([argv]))
    #print (arr[2])



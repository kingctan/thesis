from pandas import concat, read_csv
from sklearn import preprocessing
from sklearn.externals import joblib
import os
dir = os.path.dirname(__file__)
#Reads all the emotion csv's in a panda dataframe
files =["fear.csv","angry.csv","happy.csv","sad.csv","surprise.csv","disgust.csv","fear2.csv","angry2.csv","happy2.csv","sad2.csv","surprise2.csv","disgust2.csv"]
feature_cols = ["JawOpen","JawSlideRight","LeftcheekPuff","LefteyebrowLowerer","LefteyeClosed","RighteyebrowLowerer","RighteyeClosed","LipCornerDepressorLeft","LipCornerDepressorRight","LipCornerPullerLeft","LipCornerPullerRight","LipPucker","LipStretcherLeft","LipStretcherRight","LowerlipDepressorLeft","LowerlipDepressorRight","RightcheekPuff"]

data = concat([read_csv(f, decimal=',',sep=';', names = ["JawOpen","JawSlideRight","LeftcheekPuff","LefteyebrowLowerer","LefteyeClosed","RighteyebrowLowerer","RighteyeClosed","LipCornerDepressorLeft",
"LipCornerDepressorRight","LipCornerPullerLeft","LipCornerPullerRight","LipPucker","LipStretcherLeft","LipStretcherRight","LowerlipDepressorLeft","LowerlipDepressorRight","RightcheekPuff","status"]
) for f in files])
le = preprocessing.LabelEncoder()
le.fit(data.status)
data['label'] =le.transform(data.status)
joblib.dump(data, 'dataset.pkl')


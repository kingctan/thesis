from __future__ import print_function, division
# use seaborn plotting style defaults
import seaborn as sns; sns.set()
from sklearn.cross_validation import cross_val_score
from sklearn.neighbors import KNeighborsClassifier
from sklearn.tree import DecisionTreeClassifier
from sklearn import svm, preprocessing
from pandas import concat, read_csv
import os
from sklearn.externals import joblib

feature_cols = ["JawOpen","JawSlideRight","LeftcheekPuff","LefteyebrowLowerer","LefteyeClosed","RighteyebrowLowerer","RighteyeClosed","LipCornerDepressorLeft","LipCornerDepressorRight","LipCornerPullerLeft","LipCornerPullerRight","LipPucker","LipStretcherLeft","LipStretcherRight","LowerlipDepressorLeft","LowerlipDepressorRight","RightcheekPuff"]

dir = os.path.dirname(__file__)
data = joblib.load(os.path.join(dir,'dataset.pkl'))
X = data[feature_cols]
y=data.status
###############################################################################
# 10-fold cross-validations
# KNN model
def KNN_cross_val():
    knn = KNeighborsClassifier(n_neighbors=4 , weights='distance')
    print ("Knn ",cross_val_score(knn, X, y, cv=10, scoring='accuracy').mean())
# Lineair SVC
def LinSVC_cross_val():
    LinSVC = svm.LinearSVC(C=1)
    print ("Lineair SVC",cross_val_score(LinSVC, X, y, cv=10, scoring='accuracy').mean())
# SVC
def SVC_cross_val():
    SVC = svm.SVC(C=10, gamma=0.56234132519034907)
    print ("SVC ",cross_val_score(SVC, X, y, cv=10, scoring='accuracy').mean())
# Random Forest
def RandomForest_cross_val():
    rf = DecisionTreeClassifier(max_depth=11)
    print ("Random Forest ",cross_val_score(rf, X, y, cv=10, scoring='accuracy').mean())

KNN_cross_val()
LinSVC_cross_val()
SVC_cross_val()
RandomForest_cross_val()
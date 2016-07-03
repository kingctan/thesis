from __future__ import print_function, division
import seaborn as sns; sns.set()
from sklearn.neighbors import KNeighborsClassifier
from sklearn.grid_search import GridSearchCV
from sklearn.tree import DecisionTreeClassifier
import numpy as np
from sklearn import svm
import os
from sklearn.externals import joblib

feature_cols = ["JawOpen","JawSlideRight","LeftcheekPuff","LefteyebrowLowerer","LefteyeClosed","RighteyebrowLowerer","RighteyeClosed","LipCornerDepressorLeft","LipCornerDepressorRight","LipCornerPullerLeft","LipCornerPullerRight","LipPucker","LipStretcherLeft","LipStretcherRight","LowerlipDepressorLeft","LowerlipDepressorRight","RightcheekPuff"]
dir = os.path.dirname(__file__)
data = joblib.load(os.path.join(dir,'dataset.pkl'))
X = data[feature_cols]
y=data.status

def Search(clf,param_grid):
    grid = GridSearchCV(clf, param_grid, cv=10, scoring='accuracy')
    grid.fit(X, y)
    print ('Best score ' , grid.best_score_)
    print ('Best params ', grid.best_params_)

def Search_KNN() :
    knn = KNeighborsClassifier()
    weight_options = ['uniform', 'distance']
    k_range = list(range(1, 31))
    param_grid = dict(n_neighbors=k_range, weights=weight_options)
    Search(knn,param_grid)

def Search_RANDOMFOREST() :
    rf = DecisionTreeClassifier()
    depth_range = range(1, 20)
    param_grid = dict(max_depth=list(depth_range))
    Search(rf,param_grid)

def Search_LinSVC_Parameters() :
    LinSVC = svm.LinearSVC()
    C_range = np.logspace(-1,2,4)
    # The smaller C, the stronger the regularization. -> No over-fitting
    param_grid = dict(C=C_range )
    Search(LinSVC,param_grid)

def Search_SVC() :
    SVC = svm.SVC()
    #C_range = np.arange(0.1,1,0.1)
    C_range = np.logspace(-1,2,4)
    gamma_range=np.logspace(-4,1,5)
    # The smaller C, the stronger the regularization. -> No over-fitting
    param_grid = dict(C=C_range,gamma=gamma_range )
    Search(SVC,param_grid)

#Search_LinSVC_Parameters()
Search_SVC()
#Search_RANDOMFOREST()
#Search_KNN()

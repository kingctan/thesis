from __future__ import print_function, division
import matplotlib.pyplot as plt
import matplotlib.colors as colors
# use seaborn plotting style defaults
from sklearn import preprocessing
import seaborn as sns; sns.set()
import pandas as pd
import numpy as np
from sklearn.decomposition import RandomizedPCA
from sklearn.decomposition import PCA
from sklearn import preprocessing
from pandas import concat, read_csv
import os
from sklearn.externals import joblib

feature_cols = ["JawOpen","JawSlideRight","LeftcheekPuff","LefteyebrowLowerer","LefteyeClosed","RighteyebrowLowerer","RighteyeClosed","LipCornerDepressorLeft","LipCornerDepressorRight","LipCornerPullerLeft","LipCornerPullerRight","LipPucker","LipStretcherLeft","LipStretcherRight","LowerlipDepressorLeft","LowerlipDepressorRight","RightcheekPuff"]


dir = os.path.dirname(__file__)
data = joblib.load(os.path.join(dir,'dataset.pkl'))
_X = data[feature_cols]
_y=data.status
_le = preprocessing.LabelEncoder()
_le.fit(data.status)
_ynum=_le.transform(data.status)

#dimensionality reduction to get a feeling about the data
def plot_create_pca(X,y,le,ynum):
    #PCA
    pca = PCA(2)  # project from 17 to 2 dimensions
    Xproj = pca.fit_transform(X)
    plt.title("PCA")
    plt.scatter(Xproj[:, 0], Xproj[:, 1], c=ynum, edgecolor='none', alpha=0.5,    cmap=plt.cm.get_cmap('nipy_spectral', 6))
    clbr =plt.colorbar(ticks=[ 0, 1,2,3,4,5]);
    clbr.set_ticklabels(le.classes_)
    fig = plt.gcf()
    ''' local directory
    fig.savefig('C:/Users/Miguel/Desktop/graph_emotions/PCA.png')
    '''
   # fig.savefig('PCA.png')
    plt.show()

def plot_create_randomized_pca(X, y,le,ynum):
    #Randomized PCA using randomized SVD (fast and scalable)
    pca = RandomizedPCA(2)
    Xproj = pca.fit_transform(X)
    plt.title("RANDOMIZED PCA")
    plt.scatter(Xproj[:, 0], Xproj[:, 1], c=ynum, edgecolor='none', alpha=0.5,
                cmap=plt.cm.get_cmap('nipy_spectral', 6))
    clbr =plt.colorbar(ticks=[ 0, 1,2,3,4,5]);
    clbr.set_ticklabels(le.classes_)
    fig = plt.gcf()
    ''' local directory
    fig.savefig('C:/Users/Miguel/Desktop/graph_emotions/RANDOMIZEDPCA.png')
    '''
    fig.savefig('RANDOMIZEDPCA.png')
    plt.show()

plot_create_pca(_X,_y,_le,_ynum)
plot_create_randomized_pca(_X,_y,_le,_ynum)
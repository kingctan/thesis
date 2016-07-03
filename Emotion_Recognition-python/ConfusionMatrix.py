import numpy as np
import matplotlib.pyplot as plt
import os

from sklearn import svm, datasets
from sklearn.cross_validation import train_test_split
from sklearn.metrics import confusion_matrix
from sklearn.externals import joblib
from sklearn import preprocessing

feature_cols = ["JawOpen","JawSlideRight","LeftcheekPuff","LefteyebrowLowerer","LefteyeClosed","RighteyebrowLowerer","RighteyeClosed","LipCornerDepressorLeft","LipCornerDepressorRight","LipCornerPullerLeft","LipCornerPullerRight","LipPucker","LipStretcherLeft","LipStretcherRight","LowerlipDepressorLeft","LowerlipDepressorRight","RightcheekPuff"]


dir = os.path.dirname(__file__)
data = joblib.load(os.path.join(dir,'dataset.pkl'))
X = data[feature_cols]
y=data.status
le = preprocessing.LabelEncoder()
le.fit(data.status)


# Split the data into a training set and a test set
X_train, X_test, y_train, y_test = train_test_split(X, y, random_state=0)

# Run classifier, using a model that is too regularized (C too low) to see
# the impact on the results
classifier = SVC = svm.SVC(C=10, gamma=0.56234132519034907 )
y_pred = classifier.fit(X_train, y_train).predict(X_test)


def plot_confusion_matrix(cm, title, cmap=plt.cm.Blues):
    plt.imshow(cm, interpolation='nearest', cmap=cmap)
    plt.title(title)
    plt.colorbar()
    tick_marks = np.arange(len(le.classes_))
    plt.xticks(tick_marks, le.classes_, rotation=45)
    plt.yticks(tick_marks, le.classes_)
    plt.tight_layout()
    plt.ylabel('True label')
    plt.xlabel('Predicted label')
    fig = plt.gcf()
    ''' local directory
    fig.savefig('C:/Users/Miguel/Desktop/graph_emotions/'+title+'.png')
    '''
    fig.savefig(title+'.png')
    #plt.show()

# Normalize the confusion matrix by row (i.e by the number of samples
# in each class)
def compute_normalized_confusion_matrix(title,y_test,y_pred):
    cm = confusion_matrix(y_test, y_pred)
    np.set_printoptions(precision=2)
    print('Confusion matrix, without normalization')
    print(cm)
    plt.figure()
    plot_confusion_matrix(cm,'Confusion_matrix_without_normalization'+title)

    cm_normalized = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]
    print('Normalized confusion matrix')
    print(cm_normalized)
    plt.figure()
    plot_confusion_matrix(cm_normalized,'Normalized_confusion_matrix'+title )



compute_normalized_confusion_matrix('confusion_matrix_SVC', y_test ,y_pred)

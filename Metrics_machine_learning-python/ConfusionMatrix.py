import numpy as np
import matplotlib.pyplot as plt
from sklearn import metrics
from sklearn.metrics import confusion_matrix

##############################################################################
# Methode create confusion matrix
def plot_confusion_matrix(cm, title, number_labels , cmap=plt.cm.Blues):
    plt.imshow(cm, interpolation='nearest', cmap=cmap)
    plt.title(title)
    plt.colorbar()
    tick_marks = np.arange(len(number_labels))
    plt.xticks(tick_marks, number_labels, rotation=45)
    plt.yticks(tick_marks, number_labels)
    plt.tight_layout()
    plt.ylabel('True label')
    plt.xlabel('Predicted label')
    fig = plt.gcf()
    ''' local directory
    fig.savefig('C:/Users/Miguel/Desktop/confusion_matrix/'+title)
    '''
    fig.savefig(title)
    #plt.show()


##############################################################################
# Methode plot confusion matrix and normalized confusion matrix between [0,1]
def compute_normalized_confusion_matrix(title, number_labels, y_test,y_pred):
    cm = confusion_matrix(y_test, y_pred)
    np.set_printoptions(precision=2)
    print('Confusion matrix, without normalization: ' +title)
    print(cm)
    plt.figure()
    plot_confusion_matrix(cm,'Confusion_matrix_without_normalization_'+title,number_labels)

    cm_normalized = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]
    print('Normalized confusion matrix: '+title)
    print(cm_normalized)
    plt.figure()
    plot_confusion_matrix(cm_normalized,'Normalized_confusion_matrix_'+title, number_labels )
##############################################################################

csv = np.genfromtxt ('speechenumbers.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = ['1', '2','3']
compute_normalized_confusion_matrix('Speech library (recognise numbers)', labels, y ,scores)

csv = np.genfromtxt ('facial_3dbuilder.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = ['0', '1']
compute_normalized_confusion_matrix('Facial recognition (Kinect 3D builder)', labels, y ,scores)

csv = np.genfromtxt ('pca.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = ['0', '1']
compute_normalized_confusion_matrix('Facial recognition (PCA)', labels, y ,scores)

csv = np.genfromtxt ('numbersall.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = ['0', '1']
compute_normalized_confusion_matrix('Writing all numbers', labels, y ,scores)

csv = np.genfromtxt ('fingercounting_binnengrens.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = ['0', '1','2','3','4','5']
compute_normalized_confusion_matrix('Finger counting within region', labels, y ,scores)

csv = np.genfromtxt ('fingercounting_buitengrens.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = ['0', '1','2','3','4','5']
compute_normalized_confusion_matrix('Finger counting out region', labels, y ,scores)

csv = np.genfromtxt ('handwriting_testpanel.csv', delimiter=";" , dtype=None)
scores = csv[:,1]
y = csv[:,0]
labels = [ '1','2','3','4','5']
compute_normalized_confusion_matrix('Handwriting testpanel', labels, y ,scores)


csv = np.genfromtxt ('speech_testpanelverderdan2m.csv', delimiter=";")
scores = csv[:,1]
y = csv[:,0]
labels = [ 'nothing','1','2','3','4','5']
compute_normalized_confusion_matrix('Speech within 3,5m boundary', labels, y ,scores)

csv = np.genfromtxt ('speech_testpanelnietverderdan2m.csv', delimiter=";")
scores = csv[:,1]
y = csv[:,0]
labels = [ 'nothing','1','2','3','4','5']
compute_normalized_confusion_matrix('Speech out 3,5m boundary', labels, y ,scores)
import numpy as np
import matplotlib.pyplot as plt
from sklearn import metrics

##############################################################################
# Open csv's file
#csv = np.genfromtxt ('output1.csv', delimiter=";")
#csv = np.genfromtxt ('speecheTest.csv', delimiter=";")
#csv = np.genfromtxt ('facial_3dbuilder.csv', delimiter=";")
#csv = np.genfromtxt ('numberone.csv', delimiter=";")
#csv = np.genfromtxt ('numberthree.csv', delimiter=";")
#csv = np.genfromtxt ('speechFirstTest.csv', delimiter=";")
#csv = np.genfromtxt ('pca.csv', delimiter=";")

def plot_roc(title,y,scores):
    # Compute ROC curve
    fpr, tpr, thresholds = metrics.roc_curve(y, scores, pos_label=1)
    roc_auc = metrics.auc(fpr, tpr)
    # Plot ROC curve
    plt.figure()
    plt.plot(fpr, tpr, label='ROC curve (area = %0.2f)' % roc_auc)
    plt.plot([0, 1], [0, 1], 'k--')
    #x,y,k-- black dashed line
    plt.xlim([0.0, 1.0])
    plt.ylim([0.0, 1.05])
    plt.xlabel('False Positive Rate')
    plt.ylabel('True Positive Rate')
    plt.title('ROC '+title+': AUC={0:0.2f}'.format(roc_auc))

    plt.legend(loc="lower right")
    fig = plt.gcf()
    ''' local directory
    fig.savefig('C:/Users/Miguel/Desktop/curves/ROC-'+title)
    '''
    fig.savefig('ROC-'+title)
    #plt.show()

def plot_pr(title,y,scores):
    # Compute Precision-Recall curve
    average_precision = metrics.average_precision_score(y, scores)
    precision, recall, _ = metrics.precision_recall_curve(y, scores)
    # Plot Precision-Recall curve
    plt.clf()
    plt.plot(recall, precision, label='Precision-Recall curve')
    plt.xlabel('Recall')
    plt.ylabel('Precision')
    plt.ylim([0.0, 1.05])
    plt.xlim([0.0, 1.0])
    plt.title('Precision-Recall '+title+': AUC={0:0.2f}'.format(average_precision))
    plt.legend(loc="lower left")
    fig = plt.gcf()
    ''' local directory
    fig.savefig('C:/Users/Miguel/Desktop/curves/Precision-Recall-'+title)
    '''
    fig.savefig('Precision-Recall-'+title)
    #plt.show()

csv = np.genfromtxt ('numbertwo.csv', delimiter=";")
scores = csv[:,0]
y = csv[:,1]
title = "Writing number two"

plot_roc(title,y,scores)
plot_pr(title,y,scores)
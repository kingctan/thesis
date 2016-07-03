import matplotlib.pyplot as plt
import numpy as np
from sklearn import metrics

def print_report(title):
    csv = np.genfromtxt (title+'.csv', delimiter=";", dtype=None )
    scores = csv[:,0]
    y = csv[:,1]
    ##############################################################################
    # Compute classification report
    cp = metrics.classification_report(y, scores )
    np.set_printoptions(precision=2) #Number of digits of precision for floating point output
    print("Classification report:\n", cp)
    plt.figure()


list = ["facial_3dbuilder","speechFirstTest","pca"]

for n in list:
    print_report(n)
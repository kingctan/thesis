import matplotlib.pyplot as plt
from scipy.misc import imread
import csv
import glob

#Heatmap class with a create method
class HeatMap:
    def create(self,filename, person):
        x=[]
        y=[]
        with open(filename, 'r') as f:
                reader = csv.reader(f,delimiter=';')
                next(reader)
                for xwaarde,ywaarde in reader:
                    x.append(xwaarde.replace(',','.'))
                    y.append(ywaarde.replace(',','.'))
        img = imread("mediaLibrary.png",0)
        #zorder is z ordening for plotting
        plt.scatter(x,y,c ='red' ,s=30,zorder=1, alpha=0.3)
        #scalars [left, right, bottom, top]
        plt.imshow(img, zorder=0, extent=[0, 1616,  867, 0])
        #plt.show()
        fig = plt.gcf()
        fig.savefig(person+'.png')
        '''Used to create heatmaps for the testpanel
        fig.savefig('C:/Users/Miguel/Desktop/recording_data/heatmaps/'+person+'.png')
        '''

heatmap = HeatMap()
''' Used to create heatmaps for the testpanel
for filename in glob.iglob('C:/Users/Miguel/Desktop/recording_data/**/gazetrack*.csv', recursive=True):
    person = (filename.split("\\",2))[1]
    heatmap.create(filename=filename,person=person)
'''
heatmap.create(filename="gazetrack.csv",person="test-person")




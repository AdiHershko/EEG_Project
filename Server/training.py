import numpy as np
from scipy.spatial import distance
from sklearn.model_selection import train_test_split
from sklearn.neighbors import KNeighborsClassifier
from sklearn.model_selection import GridSearchCV
from sklearn.metrics import classification_report
import joblib
import os


def train():
    # train
    __location__ = os.path.realpath(
        os.path.join(os.getcwd(), os.path.dirname(__file__)))
    dataAdhd = open(os.path.join(__location__, 'allAdhd.csv'))
    dataAdhd = np.genfromtxt(dataAdhd, delimiter=',')
    dataControl = open(os.path.join(__location__, 'allControl.csv'))
    dataControl = np.genfromtxt(dataControl, delimiter=',')
    data = np.concatenate((dataAdhd, dataControl), axis=0)  # concat adhd and non adhd together
    np.take(data, np.random.permutation(data.shape[0]), axis=0, out=data)  # shuffle data
    labels = data[:, 15]  # last column, 1 for adhd and 0 for no adhd
    data = data[:, :15]

    x_train = data
    y_train = labels
    X_train, X_test, y_train, y_test = train_test_split(x_train, y_train, test_size=0.33, random_state=42)
    parameters = {'n_neighbors': [2, 4, 8]}
    clf = GridSearchCV(KNeighborsClassifier(metric=DTW), parameters, cv=3, verbose=1)
    clf.fit(X_train, y_train)
    joblib.dump(clf.best_estimator_, os.path.join(__location__, 'model.pkl'))
    # evaluate
    y_pred = clf.predict(X_test)
    print(classification_report(y_test, y_pred))


def predict(data):
    __location__ = os.path.realpath(os.path.join(os.getcwd(), os.path.dirname(__file__)))
    model = joblib.load(os.path.join(__location__, 'model.pkl'))
    prediction = model.predict(data)
    print(prediction)
    return prediction[0]


# custom metric
def DTW(a, b):
    an = a.size
    bn = b.size
    pointwise_distance = distance.cdist(a.reshape(-1, 1), b.reshape(-1, 1))
    cumdist = np.matrix(np.ones((an + 1, bn + 1)) * np.inf)
    cumdist[0, 0] = 0

    for ai in range(an):
        for bi in range(bn):
            minimum_cost = np.min([cumdist[ai, bi + 1],
                                   cumdist[ai + 1, bi],
                                   cumdist[ai, bi]])
            cumdist[ai + 1, bi + 1] = pointwise_distance[ai, bi] + minimum_cost

    return cumdist[an, bn]



array = np.array(
  [6740.170285, 1090.762319, 1127.23277, 836.9781184, 1453.655927, 1052.19358, 1007.265651, 1114.154121, 950.2640184,
   2127.347831, 1096.7244, 1197.76314, 1079.679013, 1219.983356, 914.4464512

   ])
array = np.array([array])
predict(array)

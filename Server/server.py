from scipy import signal
from flask import Flask, request, Response
import json
from flask import jsonify
import pandas as pd
import numpy as np
from scipy.spatial import distance
from sklearn.model_selection import train_test_split
from sklearn.neighbors import KNeighborsClassifier
from sklearn.model_selection import GridSearchCV
from sklearn.metrics import classification_report
import joblib
import os


app = Flask(__name__)


@app.route('/welch', methods=['POST'], endpoint='welch')
def welch():
    time_segment = int(request.form.get('time'))
    num_hz = int(request.form.get('hz'))
    channel = int(request.form.get('channel'))
    data = request.form.get('data')
    data = json.loads(data)
    data = np.array(data[channel])
    win = time_segment * num_hz
    freqs, psd = signal.welch(data, 100, nperseg=win)
    return jsonify((pd.Series(freqs).to_json(orient='values')), pd.Series(psd).to_json(orient='values'))


@app.route('/welch1d', methods=['POST'], endpoint='welch1d')
def welch():
    time_segment = int(request.form.get('time'))
    num_hz = int(request.form.get('hz'))
    data = request.form.get('data')
    data = json.loads(data)
    data = np.array(data)
    win = time_segment * num_hz
    freqs, psd = signal.welch(data, 100, nperseg=win)
    return jsonify((pd.Series(freqs).to_json(orient='values')), pd.Series(psd).to_json(orient='values'))


@app.route('/training', methods=['POST'], endpoint='training')
def training():
    train()
    return Response("OK", status=200, mimetype='application/json')


@app.route('/classify', methods=['POST'], endpoint='classify')
def classify():
    data = request.form.get('data')
    data = json.loads(data)
    data = np.array(data)
    array = np.array([data])
    result = predict(array)
    return Response(str(result), status=200, mimetype='application/json')

def train():
    # train
    numberOfParts = int(request.form.get('numberOfParts'))
    __location__ = os.path.realpath(
        os.path.join(os.getcwd(), os.path.dirname(__file__)))
    dataAdhd = open(os.path.join(__location__, 'allAdhd.csv'))
    dataAdhd = np.genfromtxt(dataAdhd, delimiter=',')
    dataControl = open(os.path.join(__location__, 'allControl.csv'))
    dataControl = np.genfromtxt(dataControl, delimiter=',')
    data = np.concatenate((dataAdhd, dataControl), axis=0)  # concat adhd and non adhd together
    np.take(data, np.random.permutation(data.shape[0]), axis=0, out=data)  # shuffle data
    labels = data[:, numberOfParts]  # last column, 1 for adhd and 0 for no adhd
    data = data[:, :numberOfParts]

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




import numpy as np
from scipy.spatial import distance
from sklearn.model_selection import train_test_split
from sklearn.neighbors import KNeighborsClassifier
from sklearn.model_selection import GridSearchCV
from sklearn.metrics import classification_report


dataAdhd = open('allAdhd.csv')
dataAdhd = np.genfromtxt(dataAdhd, delimiter=',')
dataControl = open('allControl.csv')
dataControl = np.genfromtxt(dataControl, delimiter=',')
data = np.concatenate((dataAdhd, dataControl), axis=0)
np.take(data, np.random.permutation(data.shape[0]), axis=0, out=data)
labels = data[:, 13]
data = data[:, :13]
# print(data)
x_train = data[:160, :]
y_train = labels[:160]

x_test = data[160:180, :]
y_test = labels[160:180]

# batch_size = 1
# model = Sequential()
# model.add(LSTM(10, input_shape=(13, 1), return_sequences=False, stateful=False))
# model.add(Dense(1, activation='sigmoid'))
# model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])
# model.fit(x_train, y_train, batch_size=batch_size, epochs=100,
#          validation_data=(x_test, y_test), shuffle=False)



# toy dataset
X_train, X_test, y_train, y_test = train_test_split(x_train, y_train, test_size=0.33, random_state=42)


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


# train
parameters = {'n_neighbors': [2, 4, 8]}
clf = GridSearchCV(KNeighborsClassifier(metric=DTW), parameters, cv=3, verbose=1)
clf.fit(X_train, y_train)

# evaluate
y_pred = clf.predict(X_test)
print(classification_report(y_test, y_pred))

import numpy as np
from keras import Sequential
from keras.layers import LSTM, Dense


dataAdhd = open('allAdhd.csv')
dataAdhd = np.genfromtxt(dataAdhd, delimiter = ',')
dataControl = open('allControl.csv')
dataControl = np.genfromtxt(dataControl, delimiter = ',')
data = np.concatenate((dataAdhd, dataControl), axis = 0)
np.take(data,np.random.permutation(data.shape[0]),axis=0,out=data)
labels = data[:,100]
data = data[:,:100]
print(data)
x_train = data[:160,:]
y_train = labels[:160]

x_test = data[160:180,:]
y_test = labels[160:180]


batch_size = 1
model = Sequential()
model.add(LSTM(10, input_shape=(100, 1), return_sequences=False, stateful=False))
model.add(Dense(1, activation='sigmoid'))
model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])
model.fit(x_train, y_train, batch_size=batch_size, epochs=100,
          validation_data=(x_test, y_test), shuffle=False)



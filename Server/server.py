from scipy import signal
from flask import Flask, request
import numpy as np
import json
from flask import jsonify
import pandas as pd

app = Flask(__name__)


@app.route('/welch', methods=['POST'])
def welch():
    time_segment = int(request.form.get('time'))
    num_hz = int(request.form.get('hz'))
    channel = int(request.form.get('channel'))
    data = request.form.get('data')
    data = json.loads(data)
    data = np.array(data[channel])
    win = time_segment * num_hz
    freqs, psd = signal.welch(data, 100, nperseg=win)
    return jsonify((pd.Series(freqs).to_json(orient='values')),pd.Series(psd).to_json(orient='values'))
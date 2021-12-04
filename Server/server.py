from scipy import signal
from flask import Flask, request
import numpy as np
import json
from flask import jsonify
import pandas as pd

app = Flask(__name__)


@app.route('/welch')
def welch():
    time_segment = int(request.args.get('time'))
    num_hz = int(request.args.get('hz'))
    data = request.args.get('data')
    data = json.loads(data)
    data = np.array(data[0])
    win = time_segment * num_hz
    freqs, psd = signal.welch(data, 100, nperseg=win)
    return jsonify((pd.Series(freqs).to_json(orient='values')),pd.Series(psd).to_json(orient='values'))
# Actividad Integradora 1
# Mayra Fernanda Camacho Rodríguez	A01378998
# Víctor Martínez Román			A01746361
# Melissa Aurora Fadanelli Ordaz		A01749483
# Juan Pablo Castañeda Serrano		A01752030
from flask import Flask, request, jsonify
from agent import *
from model import *

trafficModel = None
currentStep = 0

app = Flask("Traffic simulation")

@app.route('/init', methods=['POST', 'GET'])
def initModel():
    global currentStep, trafficModel, width, height

    if request.method == 'POST':
        box = int(request.form.get('box'))
        currentStep = 0

        trafficModel = TrafficModel(box)

        return jsonify({"message":"Parameters recieved, model initiated."})

@app.route('/getCars', methods=['GET'])
def getRobots():
    global trafficModel

    if request.method == 'GET':
        carPositions = []
        for (a, x, z) in trafficModel.grid.coord_iter():
            for i in a:
                dic = {}
                if isinstance(i, Car):
                    dic["id"] = str(i.unique_id)
                    dic["x"] = x
                    dic["z"] = z
                    carPositions.append(dic)

        return jsonify({'positions':carPositions})

@app.route('/getLights', methods=['GET'])
def getBoxes():
    global trafficModel

    if request.method == 'GET':
        lightPositions = []
        for (a, x, z) in trafficModel.grid.coord_iter():
            for i in a:
                dic = {}
                if isinstance(i, Traffic_Light):
                    dic["id"] = str(i.unique_id)
                    dic["x"] = x
                    dic["z"] = z
                    dic["state"] = i.state
                    dic["rotate"] = i.rotate
                    lightPositions.append(dic)
        
        return jsonify({'positions':lightPositions})

@app.route('/getShelves', methods=['GET'])
def getShelves():
    global robotModel

    if request.method == 'GET':
        shelvesPositions = []
        for (a, x, z) in robotModel.grid.coord_iter():
            if len(a) > 0:
                for i in a:
                    dic = {}
                    if isinstance(i, Anaquel):
                        dic["id"] = str(i.unique_id)
                        dic["x"] = x
                        dic["z"] = z
                        shelvesPositions.append(dic)

    return jsonify({'positions':shelvesPositions})

@app.route('/update', methods=['GET'])
def updateModel():
    global currentStep, trafficModel
    if request.method == 'GET':
        trafficModel.step()
        currentStep += 1
        return jsonify({'message':f'Model updated to step {currentStep}.', 'currentStep':currentStep})

if __name__=='__main__':
    app.run(host="0.0.0.0", port=8585, debug=True)

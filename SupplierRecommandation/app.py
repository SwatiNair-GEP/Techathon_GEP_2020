from werkzeug.contrib.fixers import ProxyFix
from flask import Flask, jsonify, request, url_for
from flask_restplus import Api, Resource, fields
import os
import requests
import json
from flask_cors import CORS
import pandas as pd 


app = Flask('supplier recommendation')
api = Api(app=app)
CORS(app, resources={r"/*": {"origins": r".*\.gep\.com"}}, support_credentials=True)
app.wsgi_app = ProxyFix(app.wsgi_app)



def get_data(item_id):
    import pyodbc
    server = 'hx67tx2ygu.database.windows.net' 
    database = 'DEV_Test2' 
    username = 'gep_sql_admin' 
    password = 'Password@123' 
    cnxn = pyodbc.connect('DRIVER={SQL Server Native Client 11.0};SERVER='+server+';DATABASE='+database+';UID='+username+';PWD='+ password)
    cursor = cnxn.cursor()
    cursor.execute('{CALL usp_GetMetricDetails(@ItemId=?)}',(item_id))
    row = cursor.fetchone() 

    df = []
    val = []

    i = 1
    while row: 
        print(row)
        df.append([row[0],row[1],row[2],float(row[3]),row[4],row[5]]) 
        row = cursor.fetchone()
    cols = ['ItemId', 'Delivery Time', 'Product Quality', 'Pricing', 'BuyerId', 'SupplierId',]
    df = pd.DataFrame(df,columns=cols)
    return df


@app.route('/recommend',methods=['POST'])
def recommend():
    data = request.get_json()
    item_id = data['item_id']
    print('item_id: ',item_id)

    df = get_data(item_id)

    # df = pd.read_excel('input.xlsx')
    print(df.head())
    print(df.info())
    
    # Assign weights to each of the attribute
    pw = 0.1
    dtw = 0.2
    pqw = 0.7

    # Calculate the Score
    df['score'] = ((pw/df['Pricing'])+(dtw/df['Delivery Time'])+(pqw * df['Product Quality']))
    print(df.head(20))

   
    # Sort the suppliers by score
    final = df.sort_values(by=['score'], ascending=False)
    print(final.head(20))

    # List to Store final result
    res = []
    for _, row in final.iterrows():
        dt = {
            'SupplierId':int(row['SupplierId']),
            'score': row['score']
        }
        res.append(dt)
    return jsonify(res)

app.run(host='0.0.0.0', port=9001, threaded=True)

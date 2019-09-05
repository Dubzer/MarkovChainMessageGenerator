from flask import Flask
from pyrogram import Client
import asyncio
from threading import Thread

api_id = 1234
api_hash = "123123123213abaabba"
chat_id = "@durov"


app = Client(session_name="markov", api_id=api_id, api_hash=api_hash)
flaskapp = Flask(__name__)


f = open("current_msg.txt", "r")
if not f:
    current_id = app.get_history_count(chat_id)
else:
    current_id = int(f.read())
global messages
i = 100


async def read_msg():
    global messages
    global i
    global current_id

    if i == 100:
        messages = app.get_history(chat_id=chat_id, offset_id=current_id, limit=100)
        print("GOT NEW HISTORY")
        i = 0
        with open('msg.txt', 'w') as file:
            file.write(str(current_id))
        print("WROTE FILE")

    i += 1
    current_id = current_id - 1
    if messages[i - 1].text is not None and messages[i - 1] is not None and messages[i - 1].forward_from is None \
            and messages[i - 1].text == messages[i - 1].text.markdown:
        return messages[i - 1].text.lower()


@flaskapp.route('/', methods=['GET'])
def result() -> str:
    msg = None
    while msg is "something" or msg is None:
        msg = asyncio.run(read_msg())

    return msg


thread = Thread(target=flaskapp.run, args=("localhost", 19566))
thread.start()

app.run()

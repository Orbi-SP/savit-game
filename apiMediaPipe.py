from flask import Flask, request, Response
import cv2
import numpy as np
import mediapipe as mp

app = Flask(__name__)

# Inicializa o MediaPipe Hands e configura os parâmetros
mp_hands = mp.solutions.hands
hands = mp_hands.Hands(
    static_image_mode=False,
    max_num_hands=1,
    min_detection_confidence=0.7,
    min_tracking_confidence=0.5
)
mp_draw = mp.solutions.drawing_utils

def detectar_mao(img):
    """
    Detecta a mão usando o MediaPipe Hands e determina se a mão está
    "Free" (mão aberta) ou "Hold" (mão fechada), com base na contagem
    de dedos estendidos.

    A lógica para dedos:
    - Para os dedos (indicador, médio, anelar e mínimo): se a ponta (landmark tip)
      estiver acima da respectiva articulação (landmark PIP), considera-se que o dedo está estendido.
    - Para o polegar: realiza uma verificação lateral (usando landmarks 4 e 2).
    
    Se nenhum dedo for considerado como estendido ou se não houver mão detectada,
    a função retorna "Free" por padrão.
    """
    # Redimensiona a imagem para processamento
    img = cv2.resize(img, (320, 240))
    img_rgb = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
    
    # Processa a imagem para detectar a mão
    results = hands.process(img_rgb)
    
    # Se nenhuma mão for detectada, retorna "Free"
    if not results.multi_hand_landmarks:
        return "Free"
    
    # Considera a primeira mão detectada (máximo 1)
    hand_landmarks = results.multi_hand_landmarks[0]
    # (Opcional) Caso deseje desenhar os landmarks, descomente a linha abaixo:
    # mp_draw.draw_landmarks(img, hand_landmarks, mp_hands.HAND_CONNECTIONS)
    
    landmarks = hand_landmarks.landmark
    extended_fingers = 0

    # Verifica se cada dedo está estendido (considerando o eixo y: menor valor significa mais alto na imagem)
    if landmarks[8].y < landmarks[6].y:      # Indicador
        extended_fingers += 1
    if landmarks[12].y < landmarks[10].y:    # Médio
        extended_fingers += 1
    if landmarks[16].y < landmarks[14].y:    # Anelar
        extended_fingers += 1
    if landmarks[20].y < landmarks[18].y:    # Mínimo
        extended_fingers += 1
    if landmarks[4].x > landmarks[2].x:      # Polegar (verificação lateral para mão direita)
        extended_fingers += 1

    return "Free" if extended_fingers > 1 else "Hold"

# Rota da API para receber imagem e retornar o estado da mão
@app.route('/', methods=['POST'])
def analisar():
    img_bytes = request.data
    npimg = np.frombuffer(img_bytes, np.uint8)
    img = cv2.imdecode(npimg, cv2.IMREAD_COLOR)
    
    if img is None:
        return Response("Imagem inválida", status=400)
    
    resultado = detectar_mao(img)
    return Response(resultado, mimetype='text/plain')

# Execução da API Flask
if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)

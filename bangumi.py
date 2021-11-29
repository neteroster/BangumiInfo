from bs4 import BeautifulSoup
import requests

REQUEST_HEADER = {'User-Agent':'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.55 Safari/537.36 Edg/96.0.1054.34'}
SUBJECT_URL = "https://bgm.tv/subject/"

class Bangumi:
    def __init__(self, id: str):
        self.id = id
        self.url = SUBJECT_URL + str(id);

        reqst = requests.get(self.url, headers=REQUEST_HEADER)
        reqst.encoding = 'utf-8'
        self.raw_text = reqst.text
        self.soup = BeautifulSoup(self.raw_text, 'html.parser')
        
        # Basic infomation
        self.summary = None
        self.original_name = None
        self.score = None

        # Information box
        self.info_box = None

    def get_raw_page(self):
        return self.raw_text

    def get_original_name(self):
        if self.original_name != None: return self.original_name

        self.original_name = self.soup.find(class_="nameSingle").contents[1].string

        return self.original_name

    def get_score(self):
        if self.score != None: return self.score

        self.score = self.soup.find(class_="number").string
        return self.score

    def get_summary(self):
        if self.summary != None: return self.summary

        summary_string = ""
        for element in self.soup.find(id="subject_summary").strings:
            summary_string += element
        self.summary = summary_string
        return summary_string
    
    def get_infobox(self):
        if self.info_box != None: return self.info_box
        self.info_box = {}

        for element in self.soup.find(id="infobox").children:
            if element.get_text().strip() == '': continue

            splited_e = element.get_text().strip().split(": ")

            self.info_box[splited_e[0]] = []

            for i in splited_e[1].split("、"):
                self.info_box[splited_e[0]].append(i)
        return self.info_box


i = Bangumi(id = 845)

print(i.get_infobox())






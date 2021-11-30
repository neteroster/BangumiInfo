import bs4
from bs4 import BeautifulSoup
import requests

REQUEST_HEADER = {'User-Agent':'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.55 Safari/537.36 Edg/96.0.1054.34'}
SUBJECT_URL = "https://bgm.tv/subject/"

class Bangumi:
    def __init__(self, id: str):
        self.id = id
        self.url = SUBJECT_URL + id

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

        # Related work
        self.related = None

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


        for element in self.soup.find(id="infobox").contents:
            if element.get_text().strip() == '': continue

            splited_e = element.get_text().strip().split(": ")

            if not splited_e[0] in self.info_box: self.info_box[splited_e[0]] = []

            for i in splited_e[1].split("、"):
                self.info_box[splited_e[0]].append(i)
        return self.info_box

    def get_related(self):
        if self.related != None: return self.related
        self.related = {}

        def is_a_related_sub(tag):
            return tag.has_attr("class") and tag.attrs["class"][0] == "sub"

        last_sub_type = ""
        for element in self.soup.find_all(is_a_related_sub):
            if element.string != None:
                last_sub_type = element.string
                self.related[last_sub_type] = []


            for down_element in element.next_siblings:
                if type(down_element) != bs4.element.Tag or down_element.attrs["class"][0] != "title": continue
                if down_element.attrs["class"] == "sep": break

                self.related[last_sub_type].append({"title": down_element.string, "id": down_element.attrs["href"].split('/')[2]})

#                self.related[element.string]["title"] = down_element.string
#                self.related[element.string]["id"] = down_element.attrs["href"].split('/')[2]
        return self.related

    def refresh_and_clean(self):

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


i = Bangumi(id = "22423")

print(i.get_related())






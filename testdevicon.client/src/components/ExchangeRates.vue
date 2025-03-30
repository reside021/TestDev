<script lang="js">
  import axios from 'axios'

  export default {
    data() {
      return {
        lastUpdateTime: new Date().toLocaleString(),
        dataRates: null,
        valutes: [],
        startDate: null,
        endDate: null,
        sortDirection: 'desc',
        isShowPeriod: false
      }
    },
    computed: {
      SortedRates() {
        return [...this.dataRates].sort((a, b) => {
          const dateA = new Date(a.date)
          const dateB = new Date(b.date)
          return this.sortDirection === 'asc' ? dateB - dateA : dateA - dateB
        })
      }
    },
    async created() {
      await this.UpdateData();
    },
    methods: {
      async UpdateData() {
        try {
          const response = await axios.get('/ExchangeRates/update');
          this.dataRates = response.data.result;
          this.CollectValutes();
        } catch (error) {
          console.log("Ошибка при получении данных:", error);
        }
      },
      async GetDataPeriod() {
        if (!this.isShowPeriod) return;

        if (!this.startDate || !this.endDate) {
          alert('Пожалуйста, выберите обе даты');
          return;
        }

        try {
          const params = {
            startDate: this.startDate,
            endDate: this.endDate
          };
          const response = await axios.get('/ExchangeRates/period', { params });
          this.dataRates = response.data.result;
          this.CollectValutes();
        } catch (error) {
          console.log("Ошибка при получении данных:", error);
        }
      },
      CollectValutes() {
        if (!this.dataRates || this.dataRates.length === 0) return;
        const uniqueValutes = new Set();
        const firstRate = this.dataRates[0];
        for (const rate in firstRate.rates) {
          uniqueValutes.add(rate);
        }
        this.valutes = Array.from(uniqueValutes).sort();
        this.lastUpdateTime = new Date().toLocaleString();
      },
      FormatDate(date) {
        return new Date(date).toLocaleDateString();
      },
      ToggleSort() {
        this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
      }
    }
  }
</script>



<template>

  <div class="top-component">
    <button @click="UpdateData()">Обновить данные с ЦБ</button>
    <label>Данные обновлены: {{ lastUpdateTime }}</label>
  </div>

  <div class="data">
    <div v-if="!dataRates" class="loading">Загрузка данных...</div>
    <div v-else-if="dataRates.length === 0" class="loading">Нет данных для отображения</div>
    <template v-else>
      <table class="data-table">
        <thead>
          <tr>
            <th id="date" @click="ToggleSort()">Дата {{ sortDirection === 'asc' ? '↓' : '↑'}}</th>
            <th v-for="valute in valutes" :key="valute">
              {{ valute }}
            </th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="record in SortedRates" :key="record.date">
            <td>{{ FormatDate(record.date).toString() }}</td>
            <td v-for="valute in valutes" :key="valute">
              {{ record.rates[valute]?.toFixed(4) ?? '-' }}
            </td>
          </tr>
        </tbody>
      </table>
    </template>  
  </div>

  <div class="selection-period">

    <div class="checkbox-block">
      <input type="checkbox" id="checkbox-period" v-model="isShowPeriod"/>
      <label for="checkbox-period">Показать за период</label>
    </div>

    <div class="date-range" v-if="isShowPeriod">
      <div class="datepicker-block">
        <label>Дата С</label>
        <input type="date" v-model="startDate" :max="endDate || ''"/>
      </div>
      <div class="datepicker-block">
        <label>Дата ПО</label>
        <input type="date" v-model="endDate" :min="startDate || ''"/>
      </div>
    </div>

    <div class="show-period-btn" v-if="isShowPeriod">
      <button @click="GetDataPeriod()">Показать за период</button>
    </div>
  </div>
</template>

<style scoped>

  .top-component{
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }


  .selection-period {
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
    min-width: 400px;
    border: 2px solid black;
    padding: 10px;
  }

  .checkbox-block{
    display: flex;
    gap: 10px;
  }

  .loading{
    padding: 20px;
    text-align: center;
    font-size: 1.2em;
  }

  .data {
    border: 2px solid black;
    margin: 20px 0;
  }

  .data-table {
    width: 100%;
    border-spacing: 0;
    font-size: 14px;
  }

  #date{
    cursor: pointer;
  }

  .data-table th,
  .data-table td {
    text-align: left;
    border-right: 2px solid black;
  }

  .data-table th:last-child,
  .data-table td:last-child {
    border-right: none;
  }

  .data-table thead tr {
    background-color: lightgray;
  }
  .data-table tbody tr:nth-child(odd) {
    background-color: whitesmoke;
  }

  .date-range {
    display: flex;
    justify-content: space-between;
    width: 100%;
  }

  .datepicker-block{
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .show-period-btn{
    display: flex;
    justify-content: flex-end;
    width: 100%;
  }
</style>

import Vue from 'vue';

/**
 * Find the element in the html (generated by linkify) based on its link relation and return the element
 * that contains the 'href' uri.
 *
 * TODO: opps, scope the HTMLElement rather than on document
 * TODO: there are bugs in this. It needs to be scoped to links and not items
 * TODO: [check: fixed] rel probably needs to be better scoped by regex too - there are false matches (eg search)
 *
 * @example
 *
 *  rel: create-form
 *
 *  <pre data-v-4a356f1e="">{
     *    <span class="key">"links":</span> [
     *        {
     *            <span class="key">"rel":</span> <span class="string">"create-form"</span>,
     *            <span class="key">"href":</span> <span class="string">"<a href="http://localhost:5000/todo/form/create" class="linkified">http://localhost:5000/todo/form/create</a>"</span>
     *        }
     *    ],
     *  </pre>
 * @param {string|RegExp} rel
 * @param {HTMLElement} el - parent scope to search
 * @returns {HTMLElement[]}
 */
const findLinkRel = (rel, el = document) => {
    return [...el.querySelectorAll('span.string')]        // <span class="string">"create-form"</span>,
        .filter(div => {
            if (typeof rel === 'string') {
                return div.innerText.includes(rel);
            } else if (rel instanceof RegExp) {
                return rel.test(div.innerText);
            }
        })
        .map(div => div.nextElementSibling.nextElementSibling); // move forward two spans <span class="string">"<a href="http...
};

/**
 * Mount a new button onto the linkify html. The button will be added before the first child.
 *
 * @remarks
 *
 * In this case, it adds the button before the '"' (quotation marks) that delineate the uri
 *
 * @example
 *
 *      <button type="button" class="btn btn-secondary btn-sm">Add</button>
 *       "
 *       <a href="http://localhost:5000/todo/form/create" class="linkified">http://localhost:5000/todo/form/create</a>
 *       "
 * @param {HTMLElement} el span element that contains the <a> anchor of the href
 * @param {{title, rel, onClick}} propsData Vue properties of the component
 */
const addButtonToHref = (el, propsData = {}) => {

    // Instructions on how to dynamically add a component to the DOM
    // https://stackoverflow.com/questions/35927664/how-to-add-dynamic-components-partials-in-vue-js
    // https://css-tricks.com/creating-vue-js-component-instances-programmatically/
    const Btn = Vue.extend({
        template: '<b-button size="sm" variant="secondary" @click="onClick(rel)">{{title}}</b-button>',
        props: {
            title: {default: 'Edit'},
            onClick: {
                type: Function, default: () => {
                }
            },
            rel: {default: 'edit-form'}
        }
    });

    const instance = new Btn({propsData});

    el && el.insertBefore(instance.$mount().$el, el.firstChild);
};

/**
 *  Add an action button on the linkify link relations html structure
 *
 * @param {string|RegExp} rel link relation to add button onto (eg self, create-form, edit-form)
 * @param {{title:string, onClick:function(rel)}} propsData Vue properties of the component
 */
const makeButtonOnLinkifyLinkRels = (rel, propsData) =>
    findLinkRel(rel).forEach(el => addButtonToHref(el, {...rel, ...propsData}));

export {makeButtonOnLinkifyLinkRels};